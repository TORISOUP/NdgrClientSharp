using System.Net;
using Dwango.Nicolive.Chat.Data;
using Dwango.Nicolive.Chat.Service.Edge;
using Google.Protobuf.WellKnownTypes;
using Moq;
using NdgrClientSharp.NdgrApi;
using R3;

namespace NdgrClientSharp.Test;

public sealed class NdgrLiveCommentFetcherSpec
{
    [Test, Timeout(5000)]
    public async Task メッセージを受信しきったらDisconnectする()
    {
        var apiClientMock = new Mock<INdgrApiClient>();

        apiClientMock
            .Setup(x =>
                x.FetchViewAtNowAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<ChunkedEntry.Types.ReadyForNext>(
                new ChunkedEntry.Types.ReadyForNext
                {
                    At = 0
                }
            ));

        apiClientMock
            .Setup(x =>
                x.FetchChunkedMessagesAsync("segment_1", It.IsAny<CancellationToken>()))
            .Returns(CreateChunkedMessagesAsync(TimeSpan.FromMilliseconds(100), "1", "2", "3"));

        apiClientMock
            .Setup(x =>
                x.FetchChunkedMessagesAsync("segment_2", It.IsAny<CancellationToken>()))
            .Returns(CreateChunkedMessagesAsync(TimeSpan.FromMilliseconds(100), "4", "5", "6"));

        apiClientMock
            .Setup(x => x.FetchViewAtAsync(It.IsAny<string>(), 0, It.IsAny<CancellationToken>()))
            .Returns(new[]
            {
                new ChunkedEntry()
                {
                    Segment = new MessageSegment()
                    {
                        Uri = "segment_1",
                        From = new Timestamp()
                        {
                            Seconds = 0 // 時間としてはめちゃくちゃだが処理は進むからOK
                        }
                    }
                },
                // 1回目はNextがある
                new ChunkedEntry()
                {
                    Next = new ChunkedEntry.Types.ReadyForNext()
                    {
                        At = 1
                    }
                }
            }.ToAsyncEnumerable());

        apiClientMock
            .Setup(x => x.FetchViewAtAsync(It.IsAny<string>(), 1, It.IsAny<CancellationToken>()))
            .Returns(new[]
            {
                // 2回目はNextがない
                new ChunkedEntry()
                {
                    Segment = new MessageSegment()
                    {
                        Uri = "segment_2",
                        From = new Timestamp()
                        {
                            Seconds = 0 // 時間としてはめちゃくちゃだが処理は進むからOK
                        }
                    }
                }
            }.ToAsyncEnumerable());

        var fetcher = new NdgrLiveCommentFetcher(apiClientMock.Object);

        // 結果の保持用
        var list = fetcher.OnMessageReceived.Select(x => x.Message.Chat.Content).ToLiveList();
        var statusList = fetcher.ConnectionStatus.ToLiveList();

        // 取得開始
        fetcher.Connect("test");
        
        // Disconnectされるまで待つ
        await fetcher.ConnectionStatus
            .Where(x => x == ConnectionState.Disconnected)
            .FirstAsync();

        Assert.Multiple(() =>
        {
            // Segmentの時間が適当なのでメッセージは意図した順番ではこない
            // ただしここでは全メッセージが受信できていることさえ見れれば良い
            CollectionAssert.AreEqual(new[] { "1", "2", "3", "4", "5", "6" }, list.Order());

            // 切断まで進んでいるはず
            CollectionAssert.AreEqual(
                new[]
                {
                    ConnectionState.Disconnected,
                    ConnectionState.Connecting,
                    ConnectionState.Connected,
                    ConnectionState.Disconnected,
                }, statusList
            );
        });
    }


    [Test, Timeout(1000)]
    public async Task 最初の取得時にServiceUnavailableのときは上限までリトライする()
    {
        var apiClientMock = new Mock<INdgrApiClient>();

        var ex = new NdgrApiClientHttpException(HttpStatusCode.ServiceUnavailable);

        // 呼び出されても例外をなげる
        apiClientMock
            .Setup(x =>
                x.FetchViewAtNowAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Throws(ex);

        var fetcher = new NdgrLiveCommentFetcher(apiClientMock.Object);

        // 最大5回までリトライ（なので最初の一回と合わせて計6回通信する）
        fetcher.MaxRetryCount = 5;
        fetcher.RetryInterval = TimeSpan.FromMilliseconds(0);

        // 結果の保持用
        var list = fetcher.OnMessageReceived.Materialize().ToLiveList();

        // 取得開始
        fetcher.Connect("");
        Assert.Multiple(() =>
        {
            // OnErrorResumeが6回発火しているはず
            Assert.That(list.Count, Is.EqualTo(6));
            Assert.That(list.All(x => x.Kind == NotificationKind.OnErrorResume), Is.True);

            // 最終的に切断している
            Assert.That(fetcher.ConnectionStatus.CurrentValue, Is.EqualTo(ConnectionState.Disconnected));
        });
    }


    [Test, Timeout(1000)]
    public async Task 最初の取得時にその他のエラー時はリトライせずに諦める()
    {
        var apiClientMock = new Mock<INdgrApiClient>();

        var ex = new NdgrApiClientHttpException(HttpStatusCode.NotFound);

        // 呼び出されても例外をなげる
        apiClientMock
            .Setup(x =>
                x.FetchViewAtNowAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Throws(ex);

        var fetcher = new NdgrLiveCommentFetcher(apiClientMock.Object);
        fetcher.MaxRetryCount = 5;
        fetcher.RetryInterval = TimeSpan.FromMilliseconds(0);

        // 結果の保持用
        var list = fetcher.OnMessageReceived.Materialize().ToLiveList();

        // 取得開始
        fetcher.Connect("");
        Assert.Multiple(() =>
        {
            // OnErrorResumeが1回発火しているはず
            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list.All(x => x.Kind == NotificationKind.OnErrorResume), Is.True);

            // 最終的に切断している
            Assert.That(fetcher.ConnectionStatus.CurrentValue, Is.EqualTo(ConnectionState.Disconnected));
        });
    }

    [Test]
    public async Task Viewを連続して取得中にServiceUnavailableが出た場合はRecconectする()
    {
        var apiClientMock = new Mock<INdgrApiClient>();

        var ex = new NdgrApiClientHttpException(HttpStatusCode.ServiceUnavailable);

        var count = 0;


        apiClientMock
            .Setup(x =>
                x.FetchViewAtNowAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<ChunkedEntry.Types.ReadyForNext>(
                new ChunkedEntry.Types.ReadyForNext
                {
                    At = 0
                }
            ));

        // 最初のViewAtの取得で例外
        apiClientMock
            .SetupSequence(x => x.FetchViewAtAsync(It.IsAny<string>(), 0, It.IsAny<CancellationToken>()))
            .Throws(ex)
            .Returns(Array.Empty<ChunkedEntry>().ToAsyncEnumerable);

        var fetcher = new NdgrLiveCommentFetcher(apiClientMock.Object);

        // 結果の保持用
        var list = fetcher.OnMessageReceived.Materialize().ToLiveList();
        var statusList = fetcher.ConnectionStatus.ToLiveList();

        // 取得開始
        fetcher.Connect("test");

        Assert.Multiple(() =>
        {
            // OnErrorResumeが1回発火しているはず
            Assert.That(list.Count, Is.EqualTo(1));
            Assert.That(list.All(x => x.Kind == NotificationKind.OnErrorResume), Is.True);

            // それぞれ呼び出されているはず
            apiClientMock
                .Verify(
                    x => x.FetchViewAtNowAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                    Times.Exactly(2)
                );

            apiClientMock
                .Verify(
                    x => x.FetchViewAtAsync(It.IsAny<string>(), 0, It.IsAny<CancellationToken>()),
                    Times.Exactly(2)
                );


            CollectionAssert.AreEqual(
                new[]
                {
                    // 1回目の接続
                    ConnectionState.Disconnected,
                    ConnectionState.Connecting,
                    ConnectionState.Connected,
                    // 再接続
                    ConnectionState.Disconnected,
                    ConnectionState.Connecting,
                    ConnectionState.Connected,
                    ConnectionState.Disconnected
                }, statusList
            );
        });
    }

    private async IAsyncEnumerable<ChunkedMessage> CreateChunkedMessagesAsync(TimeSpan delayTime, params string[] texts)
    {
        foreach (var text in texts)
        {
            await Task.Delay(delayTime);
            yield return new ChunkedMessage()
            {
                Message = new NicoliveMessage()
                {
                    Chat = new Chat()
                    {
                        Content = text
                    }
                }
            };
        }
    }
}