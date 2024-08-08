using Dwango.Nicolive.Chat.Data;
using Dwango.Nicolive.Chat.Service.Edge;
using Google.Protobuf.WellKnownTypes;
using Moq;
using NdgrClientSharp.NdgrApi;
using R3;
using ReadyForNext = Dwango.Nicolive.Chat.Service.Edge.ChunkedEntry.Types.ReadyForNext;

namespace NdgrClientSharp.Test;

public sealed class NdgrPastCommentFetcherSpec
{
    [Test, Timeout(1000)]
    public async Task 新しい方のPreviousからコメントが取得される()
    {
        var apiClientMock = new Mock<INdgrApiClient>();

        apiClientMock
            .Setup(x =>
                x.FetchViewAtNowAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<ReadyForNext>(
                new ReadyForNext
                {
                    At = 10
                }
            ));

        // 2つのPreviousが返される
        apiClientMock
            .Setup(x =>
                x.FetchViewAtAsync(It.IsAny<string>(), 10, It.IsAny<CancellationToken>()))
            .Returns(new[]
                {
                    new ChunkedEntry()
                    {
                        Previous = new MessageSegment()
                        {
                            From = new Timestamp() { Seconds = 2 },
                            Until = new Timestamp() { Seconds = 3 },
                            Uri = "first"
                        },
                    },
                    new ChunkedEntry()
                    {
                        Previous = new MessageSegment()
                        {
                            From = new Timestamp() { Seconds = 1 },
                            Until = new Timestamp() { Seconds = 2 },
                            Uri = "seconds"
                        },
                    }
                }.ToAsyncEnumerable()
            );

        apiClientMock.Setup(x =>
                x.FetchChunkedMessagesAsync("first", It.IsAny<CancellationToken>()))
            .Returns(CreateChunkedMessages("one").ToAsyncEnumerable);
        apiClientMock.Setup(x =>
                x.FetchChunkedMessagesAsync("seconds", It.IsAny<CancellationToken>()))
            .Returns(CreateChunkedMessages("two").ToAsyncEnumerable);

        var pastCommentFetcher = new NdgrPastCommentFetcher(apiClientMock.Object);

        // 取得実行
        var list = await pastCommentFetcher.FetchPastComments("", 1).ToListAsync();

        // 時刻が新しいほうが呼び出されているはず
        apiClientMock.Verify(x =>
            x.FetchChunkedMessagesAsync("first", It.IsAny<CancellationToken>()), Times.Once);
        apiClientMock.Verify(x =>
            x.FetchChunkedMessagesAsync("seconds", It.IsAny<CancellationToken>()), Times.Never);

        CollectionAssert.AreEqual(CreateChunkedMessages("one"), list);
    }

    [Test, Timeout(1000)]
    public async Task 指定した個数に達するまでコメントを連続取得する()
    {
        var apiClientMock = new Mock<INdgrApiClient>();

        apiClientMock
            .Setup(x =>
                x.FetchViewAtNowAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<ReadyForNext>(
                new ReadyForNext
                {
                    At = 10
                }
            ));

        // PreviousとBackwardを返す
        apiClientMock
            .Setup(x =>
                x.FetchViewAtAsync(It.IsAny<string>(), 10, It.IsAny<CancellationToken>()))
            .Returns(new[]
                {
                    new ChunkedEntry()
                    {
                        Backward = new BackwardSegment()
                        {
                            Until = new Timestamp() { Seconds = 2 },
                            Segment = new PackedSegment.Types.Next()
                            {
                                Uri = "segment_0"
                            }
                        },
                    },
                    new ChunkedEntry()
                    {
                        Previous = new MessageSegment()
                        {
                            From = new Timestamp() { Seconds = 2 },
                            Until = new Timestamp() { Seconds = 3 },
                            Uri = "first"
                        },
                    }
                }.ToAsyncEnumerable()
            );

        // Previousがコメントを1個返す
        apiClientMock.Setup(x =>
                x.FetchChunkedMessagesAsync("first", It.IsAny<CancellationToken>()))
            .Returns(CreateChunkedMessages("0").ToAsyncEnumerable);

        // セットアップ
        SetupBackwardSegment(apiClientMock, "segment_0", "segment_1", CreateChunkedMessages("1", "2"));
        SetupBackwardSegment(apiClientMock, "segment_1", "segment_2", CreateChunkedMessages("3", "4"));
        SetupBackwardSegment(apiClientMock, "segment_2", "segment_3", CreateChunkedMessages("5", "6"));


        var pastCommentFetcher = new NdgrPastCommentFetcher(apiClientMock.Object);

        // 4個を目標に取得
        var list = await pastCommentFetcher.FetchPastComments("", 4).ToListAsync();

        // 実際は5個取得されているはず
        Assert.That(list.Count, Is.EqualTo(5));

        // コメントも期待通り取得されている
        var resultChat = list.Select(x => x.Message.Chat.Content).ToArray();
        CollectionAssert.AreEqual(new[] { "0", "1", "2", "3", "4" }, resultChat);

        // 呼び出し数の確認
        apiClientMock.Verify(x =>
            x.FetchChunkedMessagesAsync("first", It.IsAny<CancellationToken>()), Times.Once);
        apiClientMock.Verify(x =>
            x.FetchPackedSegmentAsync("segment_0", It.IsAny<CancellationToken>()), Times.Once);
        apiClientMock.Verify(x =>
            x.FetchPackedSegmentAsync("segment_1", It.IsAny<CancellationToken>()), Times.Once);
        // ここまでは達してない
        apiClientMock.Verify(x =>
            x.FetchPackedSegmentAsync("segment_2", It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test, Timeout(1000)]
    public async Task 個数を指定しない場合は最後まで連続取得する()
    {
        var apiClientMock = new Mock<INdgrApiClient>();

        apiClientMock
            .Setup(x =>
                x.FetchViewAtNowAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<ReadyForNext>(
                new ReadyForNext
                {
                    At = 10
                }
            ));

        // PreviousとBackwardを返す
        apiClientMock
            .Setup(x =>
                x.FetchViewAtAsync(It.IsAny<string>(), 10, It.IsAny<CancellationToken>()))
            .Returns(new[]
                {
                    new ChunkedEntry()
                    {
                        Backward = new BackwardSegment()
                        {
                            Until = new Timestamp() { Seconds = 2 },
                            Segment = new PackedSegment.Types.Next()
                            {
                                Uri = "segment_0"
                            }
                        },
                    },
                    new ChunkedEntry()
                    {
                        Previous = new MessageSegment()
                        {
                            From = new Timestamp() { Seconds = 2 },
                            Until = new Timestamp() { Seconds = 3 },
                            Uri = "first"
                        },
                    }
                }.ToAsyncEnumerable()
            );

        // Previousがコメントを1個返す
        apiClientMock.Setup(x =>
                x.FetchChunkedMessagesAsync("first", It.IsAny<CancellationToken>()))
            .Returns(CreateChunkedMessages("0").ToAsyncEnumerable);

        // セットアップ
        SetupBackwardSegment(apiClientMock, "segment_0", "segment_1", CreateChunkedMessages("1", "2"));
        SetupBackwardSegment(apiClientMock, "segment_1", "segment_2", CreateChunkedMessages("3", "4"));
        SetupBackwardSegment(apiClientMock, "segment_2", null, CreateChunkedMessages("5", "6"));

        var pastCommentFetcher = new NdgrPastCommentFetcher(apiClientMock.Object);

        // 上限無しで取得
        var list = await pastCommentFetcher.FetchPastComments("").ToListAsync();

        // 7個取得されているはず
        Assert.That(list.Count, Is.EqualTo(7));

        // コメントも期待通り取得されている
        var resultChat = list.Select(x => x.Message.Chat.Content).ToArray();
        CollectionAssert.AreEqual(new[] { "0", "1", "2", "3", "4", "5", "6" }, resultChat);

        // 呼び出し数の確認
        apiClientMock.Verify(x =>
            x.FetchChunkedMessagesAsync("first", It.IsAny<CancellationToken>()), Times.Once);
        apiClientMock.Verify(x =>
            x.FetchPackedSegmentAsync("segment_0", It.IsAny<CancellationToken>()), Times.Once);
        apiClientMock.Verify(x =>
            x.FetchPackedSegmentAsync("segment_1", It.IsAny<CancellationToken>()), Times.Once);
        apiClientMock.Verify(x =>
            x.FetchPackedSegmentAsync("segment_2", It.IsAny<CancellationToken>()), Times.Once);
    }


    private ChunkedMessage[] CreateChunkedMessages(params string[] texts)
    {
        return texts.Select(x => new ChunkedMessage()
            {
                Message = new NicoliveMessage()
                {
                    Chat = new Chat()
                    {
                        Content = x
                    }
                }
            })
            .ToArray();
    }

    private void SetupBackwardSegment(Mock<INdgrApiClient> apiClientMock,
        string currentUir,
        string? nextUri,
        ChunkedMessage[] messages)
    {
        PackedSegment packedSegment;
        if (nextUri == null)
        {
            packedSegment = new PackedSegment()
            {
                Messages = { messages },
            };
        }
        else
        {
            packedSegment = new PackedSegment()
            {
                Messages = { messages },
                Next = new PackedSegment.Types.Next()
                {
                    Uri = nextUri
                }
            };
        }

        apiClientMock
            .Setup(x =>
                x.FetchPackedSegmentAsync(currentUir, It.IsAny<CancellationToken>()))
            .Returns(new ValueTask<PackedSegment>(packedSegment));
    }
}