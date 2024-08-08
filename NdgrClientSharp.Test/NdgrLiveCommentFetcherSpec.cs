using System.Net;
using Dwango.Nicolive.Chat.Service.Edge;
using Moq;
using NdgrClientSharp.NdgrApi;
using R3;

namespace NdgrClientSharp.Test;

public sealed class NdgrLiveCommentFetcherSpec
{
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
        var list = fetcher.OnNicoliveCommentReceived.Materialize().ToLiveList();

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
        var list = fetcher.OnNicoliveCommentReceived.Materialize().ToLiveList();

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


        // 呼び出されたら成功
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
        var list = fetcher.OnNicoliveCommentReceived.Materialize().ToLiveList();
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
                    ConnectionState.Connected
                }, statusList
            );
        });
    }
}