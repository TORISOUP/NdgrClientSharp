using System;
using System.Buffers;
using System.IO;

namespace NdgrClientSharp.Utilities
{
    internal sealed class NdgrProtobufStreamReader : IDisposable
    {
        private readonly MemoryStream _bufferStream = new MemoryStream();

        public void AddNewChunk(byte[] chunk, int? length = null)
        {
            // 末尾に書き込む
            _bufferStream.Position = _bufferStream.Length;
            _bufferStream.Write(chunk, 0, length ?? chunk.Length);
        }

        /// <summary>
        /// Varintの読み取り
        /// バイト列の先頭をVarint（Protocol Buffersの１データサイズの長さ）として読み取る
        ///
        /// 参考
        /// https://protobuf.dev/programming-guides/encoding/
        /// https://github.com/protocolbuffers/protobuf/blob/384fabf35a9b5d1f1502edaf9a139b1f51551a01/csharp/src/Google.Protobuf/ParsingPrimitives.cs#L720-L73
        /// This implementation is inspired by the Protocol Buffers library by Google.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NdgrProtobufStreamReaderException"></exception>
        internal (int shift, uint result)? ReadVarint()
        {
            var result = 0;
            var offset = 0;
            var shift = 0;

            _bufferStream.Position = 0;

            for (; offset < 32; offset += 7)
            {
                int b = _bufferStream.ReadByte();
                if (b == -1)
                {
                    return null;
                }

                shift++;
                result |= (b & 0x7f) << offset;
                if ((b & 0x80) == 0)
                {
                    return (shift, (uint)result);
                }
            }

            for (; offset < 64; offset += 7)
            {
                var b = _bufferStream.ReadByte();
                if (b == -1)
                {
                    return null;
                }

                shift++;

                if ((b & 0x80) == 0)
                {
                    return (shift, (uint)result);
                }
            }

            // Varintの読み取りに失敗した
            throw new NdgrProtobufStreamReaderException("Failed to read varint from the stream.");
        }

        /// <summary>
        /// Bufferの先頭からVarintを読み取り、その値に基づいてバッファからメッセージを取り出す
        /// falseが返却された場合は、まだ十分なデータを受信仕切っていないことを示す
        /// </summary>
        /// <param name="messageBuffer">戻り値がtrueの場合はProtoBuffのバイナリ列が格納されている</param>
        /// <returns>正常に読み取れたか、読み取れた場合はバイトサイズも返す</returns>
        /// <exception cref="NdgrProtobufStreamReaderException"></exception>
        public (bool isValid, int messageSize) UnshiftChunk(byte[] messageBuffer)
        {
            var readVarint = ReadVarint();
            if (readVarint == null) return (false, 0);

            var (offset, varint) = readVarint.Value;

            // offset : Varint分のバイト数
            // varint : 実際のProtoBuffのメッセージサイズ
            // offset + varint : この長さ分のバイト列がProtoBuffとして解釈するために必要
            if (offset + varint > _bufferStream.Length)
            {
                // まだ十分なデータを受信していない
                return (false, 0);
            }

            if (offset + varint > messageBuffer.Length)
            {
                // messageBufferのサイズが足りない
                throw new NdgrProtobufStreamReaderException("Message size is too large.");
            }


            // Read the message bytes directly into the array
            _bufferStream.Position = offset;
            _bufferStream.Read(messageBuffer, 0, (int)varint);

            // Shift the buffer content
            var remainingLength = (int)(_bufferStream.Length - offset - varint);
            using var rentedBuffer = new PooledArray<byte>(remainingLength);

            _bufferStream.Position = offset + varint;
            _bufferStream.Read(rentedBuffer.Array, 0, remainingLength);

            // Reset and refill the stream with the remaining data
            _bufferStream.SetLength(0);
            _bufferStream.Write(rentedBuffer.Array, 0, remainingLength);

            return (true, (int)varint);
        }

        public void Dispose()
        {
            _bufferStream.Dispose();
        }
    }

    public class NdgrProtobufStreamReaderException : Exception
    {
        public NdgrProtobufStreamReaderException(string message) : base(message)
        {
        }
    }
}