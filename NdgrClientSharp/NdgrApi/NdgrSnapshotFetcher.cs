using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Dwango.Nicolive.Chat.Service.Edge;

namespace NdgrClientSharp.NdgrApi
{
    public sealed class NdgrSnapshotFetcher : IDisposable
    {
        private readonly INdgrApiClient _ndgrApiClient;
        private readonly bool _needDisposeNdgrApiClient;
        private bool _isDisposed;
        private readonly object _gate = new object();
        private readonly CancellationTokenSource _mainCts = new CancellationTokenSource();

        public NdgrSnapshotFetcher(INdgrApiClient? ndgrApiClient = null)
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
        /// 現在のスナップショットを取得する
        /// </summary>
        public async IAsyncEnumerable<ChunkedMessage> FetchCurrentSnapshotAsync(
            string viewUri,
            [EnumeratorCancellation] CancellationToken token)
        {
            lock (_gate)
            {
                if (_isDisposed) throw new ObjectDisposedException(nameof(NdgrSnapshotFetcher));
            }

            var ct = CancellationTokenSource.CreateLinkedTokenSource(_mainCts.Token, token).Token;

            var now = await _ndgrApiClient.FetchViewAtNowAsync(viewUri, ct);

            await foreach (var ce in _ndgrApiClient.FetchViewAtAsync(viewUri, now.At, ct))
            {
                if (ce.EntryCase != ChunkedEntry.EntryOneofCase.Backward) continue;

                var backward = ce.Backward;
                await foreach (var s in _ndgrApiClient.FetchChunkedMessagesAsync(backward.Snapshot.Uri, ct))
                {
                    yield return s;
                }
            }
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