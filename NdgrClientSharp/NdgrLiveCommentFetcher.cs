using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Dwango.Nicolive.Chat.Data;
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
        /// 番組が終了したことを通知する
        /// すでに終了済みの番組については発火しない
        /// </summary>
        public Observable<Unit> OnProgramEnded { get; }

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

        /// <summary>
        /// 現在受信中のSegment一覧
        /// </summary>
        public IEnumerable<string> CurrentReceivingSegments
        {
            get
            {
                lock (_receivedMessages)
                {
                    return _receivedMessages.Keys.ToArray();
                }
            }
        }

        private readonly INdgrApiClient _ndgrApiClient;
        private readonly object _gate = new object();
        private readonly Subject<ChunkedMessage> _messageSubject = new Subject<ChunkedMessage>();
        private readonly bool _needDisposeNdgrApiClient;

        private readonly SynchronizedReactiveProperty<ConnectionState>
            _connectionStatus = new SynchronizedReactiveProperty<ConnectionState>(ConnectionState.Disconnected);

        private CancellationTokenSource? _mainCts;
        private bool _isDisposed;
        private string _latestViewApiUri = string.Empty;
        private uint _fetchingSegmentCount = 0;

        private readonly Dictionary<string, HashSet<string>> _receivedMessages =
            new Dictionary<string, HashSet<string>>();

        public NdgrLiveCommentFetcher(INdgrApiClient? ndgrApiClient = null)
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

            OnProgramEnded = _messageSubject
                .Where(x => x.State?.ProgramStatus?.State is ProgramStatus.Types.State.Ended)
                .AsUnitObservable()
                .Share();
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
                _receivedMessages.Clear();

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

                    // コメント取得のためのSegmentおよびNextの取得処理
                    Forget(FetchLoopAsync(viewApiUri, next.At, ct));
                    return;
                }
                catch (OperationCanceledException)
                {
                    // nothing
                    return;
                }
                catch (WebException w) when (w.Status == WebExceptionStatus.RequestCanceled)
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
                    var receivedSegments = new HashSet<string>();

                    // SegmentおよびNextの監視
                    await foreach (var chunk in chunks)
                    {
                        switch (chunk.EntryCase)
                        {
                            case ChunkedEntry.EntryOneofCase.Segment:
                                // Segment（コメント本文を返すAPI情報）
                                var segment = chunk.Segment;
                                receivedSegments.Add(segment.Uri);
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

                    // ReceivedSegmentsに含まれないものはDictionaryから削除
                    lock (_receivedMessages)
                    {
                        foreach (var key in _receivedMessages.Keys.ToArray())
                        {
                            if (!receivedSegments.Contains(key))
                            {
                                _receivedMessages[key]?.Clear();
                                _receivedMessages.Remove(key);
                            }
                        }
                    }

                    if (!hasNext)
                    {
                        // Nextがない場合はSegmentの全受信を待ってから終了
                        // だが実際はNextは無限に存在し続けるぽいからここが実行されることはないはず
                        while (_fetchingSegmentCount > 0)
                        {
                            // 適当に終わりそうな時間だけ待つ
                            await Task.Delay(TimeSpan.FromSeconds(3), ct);
                        }

                        Disconnect();
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // nothing
            }
            catch (WebException w) when (w.Status == WebExceptionStatus.RequestCanceled)
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

                HashSet<string> hashSet;

                lock (_receivedMessages)
                {
                    if (!_receivedMessages.TryGetValue(segment.Uri, out var set))
                    {
                        hashSet = new HashSet<string>();
                        _receivedMessages.Add(segment.Uri, hashSet);
                    }
                    else
                    {
                        hashSet = set;
                    }
                }

                var uri = segment.Uri;
                await foreach (var chunkedMessage in _ndgrApiClient.FetchChunkedMessagesAsync(uri, ct))
                {
                    lock (_gate)
                    {
                        var meta = chunkedMessage.Meta;
                        if (meta != null)
                        {
                            // Metaが存在する場合は重複チェック
                            if (hashSet.Add(chunkedMessage.Meta.Id))
                            {
                                _messageSubject.OnNext(chunkedMessage);
                            }
                        }
                        else
                        {
                            // Metaが存在しない場合は素通し
                            _messageSubject.OnNext(chunkedMessage);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (WebException w) when (w.Status == WebExceptionStatus.RequestCanceled)
            {
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
            catch (WebException w) when (w.Status == WebExceptionStatus.RequestCanceled)
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