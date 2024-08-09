using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Dwango.Nicolive.Chat.Service.Edge;
using NdgrClientSharp.NdgrApi;
using R3;

namespace NdgrClientSharp
{
    /// <summary>
    /// ニコ生の生放送中のコメントを取得する
    /// </summary>
    public sealed class NdgrLiveCommentFetcher : IDisposable
    {
        /// <summary>
        /// ・受信したコメント情報が発信される
        /// ・実行中に発生したエラーはOnErrorResumeとして発行
        /// ・DisconnectしてもOnCompletedは発行されない
        /// ・DisposeするとOnCompletedが発行される
        /// </summary>
        public Observable<ChunkedMessage> OnMessageReceived => _messageSubject;

        /// <summary>
        /// 接続状態
        /// </summary>
        public ReadOnlyReactiveProperty<ConnectionState> ConnectionStatus => _connectionStatus;

        /// <summary>
        /// コメント取得エラー発生時の最大リトライ回数
        /// 0の場合はリトライしない
        /// </summary>
        public int MaxRetryCount { get; set; } = 5;

        /// <summary>
        /// コメント取得エラー発生時のリトライ間隔
        /// </summary>
        public TimeSpan RetryInterval { get; set; } = TimeSpan.FromSeconds(2);

        private readonly INdgrApiClient _ndgrApiClient;
        private readonly object _gate = new object();
        private readonly Subject<ChunkedMessage> _messageSubject = new Subject<ChunkedMessage>();
        private readonly TimeProvider _timeProvider;
        private readonly bool _needDisposeNdgrApiClient;

        private readonly SynchronizedReactiveProperty<ConnectionState>
            _connectionStatus = new SynchronizedReactiveProperty<ConnectionState>(ConnectionState.Disconnected);

        private CancellationTokenSource? _mainCts;
        private bool _isDisposed;

        // サーバ時刻とローカル時刻の差分
        private int _offsetSeconds = 0;
        private string _latestViewApiUri = string.Empty;
        private uint _fetchingSegmentCount = 0;

        /// <summary>
        /// ローカルPCの時刻のズレを補正し、サーバ側に合わせた時刻
        /// </summary>
        private DateTimeOffset OffsetCurrentTime =>
            _timeProvider.GetUtcNow().AddSeconds(_offsetSeconds);

        public NdgrLiveCommentFetcher(INdgrApiClient? ndgrApiClient = null, TimeProvider? timeProvider = null)
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

            _timeProvider = timeProvider ?? TimeProvider.System;
        }


        /// <summary>
        /// ViewAPIに接続してコメント取得を開始する
        /// 接続中またはすでに接続済みの場合は何もしない
        /// </summary>
        public void Connect(string viewApiUri)
        {
            lock (_gate)
            {
                if (_isDisposed) throw new ObjectDisposedException(nameof(NdgrLiveCommentFetcher));
                if (ConnectionStatus.CurrentValue != ConnectionState.Disconnected)
                {
                    return;
                }

                _mainCts = new CancellationTokenSource();
                _connectionStatus.Value = ConnectionState.Connecting;
                _latestViewApiUri = viewApiUri;
                _fetchingSegmentCount = 0;

                // 取得処理開始
                Forget(FetchStartAsync(viewApiUri, _mainCts.Token));
            }
        }

        /// <summary>
        /// コメント取得を停止する
        /// </summary>
        public void Disconnect()
        {
            lock (_gate)
            {
                if (_isDisposed) return;

                _mainCts?.Cancel();
                _mainCts?.Dispose();
                _mainCts = null;
                _connectionStatus.Value = ConnectionState.Disconnected;
                _fetchingSegmentCount = 0;
            }
        }


        /// <summary>
        /// 最後に接続していたViewAPIに再接続する
        /// すでに接続済みの場合は切断してから再接続する
        /// </summary>
        public void Reconnect()
        {
            lock (_gate)
            {
                if (_isDisposed) throw new ObjectDisposedException(nameof(NdgrLiveCommentFetcher));
                if (ConnectionStatus.CurrentValue == ConnectionState.Connected)
                {
                    Disconnect();
                }

                if (_latestViewApiUri == string.Empty)
                {
                    throw new InvalidOperationException("No latest ViewAPI URI. Cannot reconnect.");
                }

                Connect(_latestViewApiUri);
            }
        }


        public void Dispose()
        {
            lock (_gate)
            {
                if (_isDisposed) return;
                _isDisposed = true;

                Disconnect();

                if (_needDisposeNdgrApiClient)
                {
                    _ndgrApiClient.Dispose();
                }

                _messageSubject.Dispose();
                _connectionStatus.Dispose();
            }
        }

        #region AsyncMainAction

        private async ValueTask FetchStartAsync(string viewApiUri, CancellationToken ct)
        {
            var tryCount = 0;

            do
            {
                try
                {
                    // 次の取得するべきViewのunixtimeを取得
                    var next = await _ndgrApiClient.FetchViewAtNowAsync(viewApiUri, ct);

                    if (_connectionStatus.Value == ConnectionState.Connecting)
                    {
                        // 取得成功したら接続済にする
                        _connectionStatus.Value = ConnectionState.Connected;
                    }

                    // サーバ時刻とローカル時刻の差分を保存して補正できるようにしておく
                    _offsetSeconds = (int)(next.At - _timeProvider.GetUtcNow().ToUnixTimeSeconds());
                    
                    // コメント取得のためのSegmentおよびNextの取得処理
                    Forget(FetchLoopAsync(viewApiUri, next.At, ct));
                    return;
                }
                catch (OperationCanceledException)
                {
                    // nothing
                    return;
                }
                catch (NdgrApiClientHttpException ex)
                {
                    lock (_gate)
                    {
                        _messageSubject.OnErrorResume(ex);
                    }

                    // 503だったときはちょっとまってリトライしてみる
                    if (ex.HttpStatusCode == HttpStatusCode.ServiceUnavailable)
                    {
                        await Task.Delay(RetryInterval, ct);
                    }
                    else
                    {
                        // 503以外のエラーはリトライしない
                        Disconnect();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    lock (_gate)
                    {
                        if (!_messageSubject.IsDisposed)
                        {
                            _messageSubject.OnErrorResume(ex);
                        }
                    }

                    Disconnect();
                    return;
                }
            } while (++tryCount <= MaxRetryCount);

            // リトライしてもダメだったらDisconnect
            Disconnect();
        }


        /// <summary>
        /// コメント取得のためのSegmentおよびNextの取得処理
        /// </summary>
        private async ValueTask FetchLoopAsync(string viewApiUri, long unixTime, CancellationToken ct)
        {
            var targetTime = unixTime;
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var hasNext = false;
                    var chunks = _ndgrApiClient.FetchViewAtAsync(viewApiUri, targetTime, ct);
                    // SegmentおよびNextの監視
                    await foreach (var chunk in chunks)
                    {
                        switch (chunk.EntryCase)
                        {
                            case ChunkedEntry.EntryOneofCase.Segment:
                                // Segment（コメント本文を返すAPI情報）
                                var segment = chunk.Segment;
                                Forget(FetchSegmentAsync(segment, ct));

                                break;
                            case ChunkedEntry.EntryOneofCase.Next:

                                // 次のView取得情報
                                targetTime = chunk.Next.At;
                                hasNext = true;
                                break;

                            // 未使用
                            case ChunkedEntry.EntryOneofCase.None:
                            case ChunkedEntry.EntryOneofCase.Backward:
                            case ChunkedEntry.EntryOneofCase.Previous:
                                break;

                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    ct.ThrowIfCancellationRequested();

                    if (!hasNext)
                    {
                        // Nextがない場合はSegmentの全受信を待ってから終了
                        
                        while (_fetchingSegmentCount > 0)
                        {
                            ct.ThrowIfCancellationRequested();
                            await Task.Yield();
                        }

                        Disconnect();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // nothing
            }
            catch (NdgrApiClientHttpException e)
            {
                lock (_gate)
                {
                    if (!_messageSubject.IsDisposed)
                    {
                        _messageSubject.OnErrorResume(e);
                    }

                    switch (e.HttpStatusCode)
                    {
                        case HttpStatusCode.ServiceUnavailable:
                            // 503の場合はリトライできる可能性あり
                            Reconnect();
                            break;
                        default:
                            // リカバリ不可能な場合は動作停止
                            Disconnect();
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                // エラーを発行してDisconnect
                lock (_gate)
                {
                    if (!_messageSubject.IsDisposed)
                    {
                        _messageSubject.OnErrorResume(e);
                    }
                }

                Disconnect();
            }
        }

        private async ValueTask FetchSegmentAsync(MessageSegment segment, CancellationToken ct = default)
        {
            try
            {
                _fetchingSegmentCount++;

                // この時刻以降のコメント情報が取得できる
                // ただし滑らかなコメント受信のためにはFromの1秒くらい前にprefetchしておく必要がある
                var from = segment.From.ToDateTimeOffset();

                // fromの1秒前
                var waitTargetTime　= from.AddSeconds(-1);

                // 補正済みの現在時刻
                var now =　OffsetCurrentTime;

                // fromの1秒前まで待機
                if (waitTargetTime > now)
                {
                    await Task.Delay(waitTargetTime - now, ct);
                }

                var uri = segment.Uri;

                await foreach (var chunkedMessage in _ndgrApiClient.FetchChunkedMessagesAsync(uri, ct))
                {
                    lock (_gate)
                    {
                        _messageSubject.OnNext(chunkedMessage);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // nothing
            }
            catch (Exception e)
            {
                lock (_gate)
                {
                    if (!_messageSubject.IsDisposed)
                    {
                        _messageSubject.OnErrorResume(e);
                    }
                }
                // Segmentは取得失敗しても継続する
            }
            finally
            {
                _fetchingSegmentCount--;
            }
        }


        private async void Forget(ValueTask task)
        {
            try
            {
                await task;
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
            catch (Exception e)
            {
                lock (_gate)
                {
                    if (!_messageSubject.IsDisposed)
                    {
                        _messageSubject.OnErrorResume(e);
                    }
                }
            }
        }

        #endregion
    }

    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected
    }
}