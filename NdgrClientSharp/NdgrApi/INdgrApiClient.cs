using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dwango.Nicolive.Chat.Service.Edge;
using ReadyForNext = Dwango.Nicolive.Chat.Service.Edge.ChunkedEntry.Types.ReadyForNext;

namespace NdgrClientSharp.NdgrApi
{
    public interface INdgrApiClient : IDisposable
    {
        /// <summary>
        /// GET /api/view/v4/:view?at=now
        /// </summary>
        ValueTask<ReadyForNext> FetchViewAtNowAsync(string viewApiUri,
            CancellationToken token = default);

        /// <summary>
        /// GET /api/view/v4/:view?at=unixtime
        /// 指定時刻付近のコメント取得のための情報を取得する
        /// backward,previous,segment,nextが非同期的に返ってくる
        /// </summary>
        IAsyncEnumerable<ChunkedEntry> FetchViewAtAsync(string viewApiUri,
            long unixTime,
            CancellationToken token = default);

        /// <summary>
        /// ChunkedMessage（コメント）を取得する
        /// Segment, Previous, Snapshot APIが対応
        /// </summary>
        IAsyncEnumerable<ChunkedMessage> FetchChunkedMessagesAsync(
            string apiUri,
            CancellationToken token = default);

        /// <summary>
        /// PackedSegmentを取得する
        /// </summary>
        ValueTask<PackedSegment> FetchPackedSegmentAsync(string uri, CancellationToken token = default);
    }


    public abstract class NdgrApiClientException : Exception
    {
        protected NdgrApiClientException(string message) : base(message)
        {
        }
    }
}