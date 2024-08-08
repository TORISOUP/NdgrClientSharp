using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Dwango.Nicolive.Chat.Service.Edge;
using NdgrClientSharp.Utilities;
using ReadyForNext = Dwango.Nicolive.Chat.Service.Edge.ChunkedEntry.Types.ReadyForNext;

namespace NdgrClientSharp.NdgrApi
{
    public class NdgrApiClient : INdgrApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly bool _needDisposeHttpClient;
        private readonly CancellationTokenSource _mainCts = new CancellationTokenSource();

        public NdgrApiClient()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "ndgr-client-sharp");
            _needDisposeHttpClient = true;
        }

        /// <summary>
        /// HttpClientを指定して初期化する
        /// このHttpClientのDisposeはNdgrApiClientでは行わない
        /// </summary>
        public NdgrApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _needDisposeHttpClient = false;
        }

        private static string TrimQuery(string uri)
        {
            // URIからクエリパラメータを除外
            return uri.Split('?')[0];
        }


        #region view api

        /// <summary>
        /// コメント取得の起点
        /// GET /api/view/v4/:view?at=now を実行して、次のコメント取得のための情報を取得する
        /// クエリパラメータはTrimしてから実行するため含まれていても問題ない
        /// </summary>
        public async ValueTask<ReadyForNext> FetchViewAtNowAsync(string viewApiUri, CancellationToken token = default)
        {
            var ct = CreateLinkedToken(token);

            // URIからクエリパラメータを除外
            var uri = TrimQuery(viewApiUri);

            var response = await _httpClient.GetAsync(
                    $"{uri}?at=now",
                    HttpCompletionOption.ResponseHeadersRead, ct)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new NdgrApiClientHttpException(response.StatusCode);
            }

            await foreach (var chunk in ReadProtoBuffBytesAsync(await response.Content.ReadAsStreamAsync())
                               .WithCancellation(ct))
            {
                var message = ChunkedEntry.Parser.ParseFrom(chunk);

                // at=nowの場合はほぼ現在時刻のunixtimeが返ってくる
                return message.Next;
            }

            throw new NdgrApiClientByteReadException("Failed to read bytes from stream.");
        }


        /// <summary>
        /// 指定時刻付近のコメント取得のための情報を取得する
        /// backward,previous,segment,nextが非同期的に返ってくる
        /// </summary>
        public async IAsyncEnumerable<ChunkedEntry> FetchViewAtAsync(string viewApiUri,
            long unixTime,
            [EnumeratorCancellation] CancellationToken token = default)
        {
            var ct = CreateLinkedToken(token);

            var uri = TrimQuery(viewApiUri);
            var response =
                await _httpClient.GetAsync(
                        $"{uri}?at={unixTime}",
                        HttpCompletionOption.ResponseHeadersRead, ct)
                    .ConfigureAwait(false);
            ;

            if (!response.IsSuccessStatusCode)
            {
                throw new NdgrApiClientHttpException(response.StatusCode);
            }

            var stream = await response.Content.ReadAsStreamAsync();
            await foreach (var chunk in ReadProtoBuffBytesAsync(stream, ct))
            {
                var message = ChunkedEntry.Parser.ParseFrom(chunk);
                yield return message;
            }
        }

        #endregion


        /// <summary>
        /// ChunkedMessage（コメント）を取得する
        /// Segment, Previous APIが対応
        /// </summary>
        public async IAsyncEnumerable<ChunkedMessage> FetchChunkedMessagesAsync(
            string apiUri,
            [EnumeratorCancellation] CancellationToken token = default)
        {
            var ct = CreateLinkedToken(token);

            await using var response = await _httpClient.GetStreamAsync(apiUri).ConfigureAwait(false);
            await foreach (var chunk in ReadProtoBuffBytesAsync(response, ct))
            {
                var message = ChunkedMessage.Parser.ParseFrom(chunk);
                yield return message;
            }
        }

        /// <summary>
        /// PackedSegmentを取得する
        /// </summary>
        public async ValueTask<PackedSegment> FetchPackedSegmentAsync(
            string uri,
            CancellationToken token = default)
        {
            var ct = CreateLinkedToken(token);
            var response = await _httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, ct)
                .ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var message = PackedSegment.Parser.ParseFrom(await response.Content.ReadAsStreamAsync());
            return message;
        }
        
        #region read bytes utils

        /// <summary>
        /// Streamから非同期的に生のバイト配列を読み取る
        /// </summary>
        private static async IAsyncEnumerable<byte[]> ReadRawBytesAsync(
            Stream stream,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            using var reader = new BinaryReader(stream);
            var buffer = new byte[2048];
            while (true)
            {
                var read = await reader.BaseStream.ReadAsync(buffer, 0, buffer.Length, ct)
                    .ConfigureAwait(false);
                if (read == 0)
                {
                    yield break;
                }

                yield return buffer[..read];
            }
        }

        /// <summary>
        /// Streamを「ProtoBuffのメッセージとして解釈可能なバイト配列単位」で読み取る
        /// </summary>
        private static async IAsyncEnumerable<byte[]> ReadProtoBuffBytesAsync(
            Stream stream,
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            using var reader = new ProtobufStreamReader();
            await foreach (var chunk in ReadRawBytesAsync(stream).WithCancellation(ct))
            {
                reader.AddNewChunk(chunk);
                while (true)
                {
                    var message = reader.UnshiftChunk();
                    if (message == null)
                    {
                        break;
                    }

                    yield return message;
                }
            }
        }

        #endregion

        private CancellationToken CreateLinkedToken(CancellationToken ct)
        {
            if (!ct.CanBeCanceled) return _mainCts.Token;
            return CancellationTokenSource.CreateLinkedTokenSource(_mainCts.Token, ct).Token;
        }

        public void Dispose()
        {
            if (_needDisposeHttpClient)
            {
                _httpClient.Dispose();
            }

            _mainCts.Cancel();
            _mainCts.Dispose();
        }
    }


    public sealed class NdgrApiClientByteReadException : NdgrApiClientException
    {
        public NdgrApiClientByteReadException(string message) : base(message)
        {
        }
    }


    public sealed class NdgrApiClientHttpException : Exception
    {
        public HttpStatusCode HttpStatusCode { get; }

        public NdgrApiClientHttpException(HttpStatusCode httpStatusCode)
        {
            HttpStatusCode = httpStatusCode;
        }

        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(HttpStatusCode)}: {HttpStatusCode}";
        }
    }
}