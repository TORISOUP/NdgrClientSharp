using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Dwango.Nicolive.Chat.Service.Edge;
using NdgrClientSharp.NdgrApi;
using R3;

namespace NdgrClientSharp
{
    /// <summary>
    /// 過去のコメントを取得する
    /// </summary>
    public sealed class NdgrPastCommentFetcher : IDisposable
    {
        private readonly INdgrApiClient _ndgrApiClient;
        private readonly bool _needDisposeNdgrApiClient;
        private bool _isDisposed;
        private readonly object _gate = new object();
        private readonly CancellationTokenSource _mainCts = new CancellationTokenSource();

        public NdgrPastCommentFetcher(INdgrApiClient? ndgrApiClient = null)
        {
            if (ndgrApiClient == null)
            {
                _ndgrApiClient = new NdgrApiClient();
                _needDisposeNdgrApiClient = true;
            }
            else
            {
                _ndgrApiClient = ndgrApiClient;
                _needDisposeNdgrApiClient = false;
            }
        }
        
        /// <summary>
        /// 過去のコメントを取得する
        /// </summary>
        /// <param name="viewApiUri">ViewURI</param>
        /// <param name="inaccurateLimit">
        /// 取得数の目安。コメントはまとまった単位で取得するため、この数値を超える数のうちの最小回数まで取得する。
        /// nullを指定した場合はすべてのコメントを取得する。</param>
        /// <returns></returns>
        public Observable<ChunkedMessage> FetchPastComments(string viewApiUri, int? inaccurateLimit = 100)
        {
            lock (_gate)
            {
                if (_isDisposed) throw new ObjectDisposedException(nameof(NdgrPastCommentFetcher));
            }

            return Observable.Create<ChunkedMessage>(async (o, ct) =>
            {
                var token = CancellationTokenSource.CreateLinkedTokenSource(_mainCts.Token, ct).Token;

                try
                {
                    var sendCount = 0;

                    var now = await _ndgrApiClient.FetchViewAtNowAsync(viewApiUri, token);
                    var previous = new List<MessageSegment>();
                    BackwardSegment? backward = null;

                    await foreach (var chunkedEntry in _ndgrApiClient.FetchViewAtAsync(viewApiUri, now.At, token))
                    {
                        if (chunkedEntry.EntryCase == ChunkedEntry.EntryOneofCase.Previous)
                        {
                            previous.Add(chunkedEntry.Previous);
                        }
                        else if (chunkedEntry.EntryCase == ChunkedEntry.EntryOneofCase.Backward)
                        {
                            backward = chunkedEntry.Backward;
                        }
                        else if (chunkedEntry.EntryCase == ChunkedEntry.EntryOneofCase.Segment)
                        {
                            // 通常のSegmentが来た時点で止める
                            break;
                        }
                    }


                    // PreviousSegmentの新しい側から取得
                    foreach (var p in previous.OrderByDescending(x => x.Until))
                    {
                        await foreach (var chunkedMessage in _ndgrApiClient.FetchChunkedMessagesAsync(p.Uri, token))
                        {
                            o.OnNext(chunkedMessage);
                            sendCount++;
                        }

                        if (sendCount >= inaccurateLimit)
                        {
                            o.OnCompleted();
                            return;
                        }
                    }

                    if (backward != null)
                    {
                        var uri = backward.Segment.Uri;
                        // Previousよりさらに過去のコメントの取得
                        while (!token.IsCancellationRequested)
                        {
                            var packedSegment = await _ndgrApiClient.FetchPackedSegmentAsync(uri, token);
                            foreach (var message in packedSegment.Messages)
                            {
                                o.OnNext(message);
                                sendCount++;
                            }

                            if (sendCount >= inaccurateLimit)
                            {
                                o.OnCompleted();
                                return;
                            }

                            if (packedSegment.Next != null)
                            {
                                uri = packedSegment.Next.Uri;
                                continue;
                            }

                            break;
                        }
                    }

                    o.OnCompleted();
                }
                catch (Exception ex)
                {
                    if (ex is OperationCanceledException)
                    {
                        o.OnCompleted();
                    }
                    else
                    {
                        o.OnCompleted(ex);
                    }
                }
            });
        }


        public void Dispose()
        {
            lock (_gate)
            {
                if (_isDisposed) return;
                _isDisposed = true;
                _mainCts.Cancel();
                _mainCts.Dispose();

                if (_needDisposeNdgrApiClient)
                {
                    _ndgrApiClient.Dispose();
                }
            }
        }
    }
}