using System;
using System.IO;

namespace NdgrClientSharp.Utilities
{

    /// <summary>
    /// Protocol BufferのBase 128 Varintsを読み取り、メッセージを取り出す
    /// https://protobuf.dev/programming-guides/encoding/
    /// </summary>
    internal sealed class ProtobufStreamReader : IDisposable
    {
        private readonly MemoryStream _bufferStream = new MemoryStream();

        public void AddNewChunk(byte[] chunk)
        {
            _bufferStream.Write(chunk, 0, chunk.Length);
        }

        private (int offset, uint result)? ReadVariant()
        {
            var offset = 0;
            uint result = 0;
            var shift = 0;

            _bufferStream.Position = 0;

            while (true)
            {
                var b = _bufferStream.ReadByte();
                if (b == -1) return null; // まだデータが揃ってない

                offset++;
                result |= (uint)(b & 0x7F) << shift;
                shift += 7;

                if (offset > 5)
                {
                    // int32を超える大きさのメッセージは来ないはずなのでなにかがおかしい
                    // バッファをクリアして処理を中断
                    _bufferStream.SetLength(0);
                    return null;
                }

                if ((b & 0x80) == 0)
                {
                    return (offset, result);
                }
            }
        }

        public byte[]? UnshiftChunk()
        {
            var readVarint = ReadVariant();
            if (readVarint == null) return null;

            var (offset, varint) = readVarint.Value;

            if (offset + varint > _bufferStream.Length)
            {
                // varintの値がバッファのサイズを超える場合はまだデータが揃っていない
                return null;
            }

            if (varint > int.MaxValue)
            {
                // int.MaxValueを超えるサイズのメッセージはこないはず
                // なにかがおかしいのでバッファをクリアして処理を中断
                _bufferStream.SetLength(0);
                return null;
            }

            
            // TODO: メモリ効率の最適化
            var message = new byte[varint];

            _bufferStream.Position = offset;
            _bufferStream.Read(message, 0, (int)varint);

            var remainingBuffer = new byte[_bufferStream.Length - offset - varint];
            _bufferStream.Position = offset + varint;
            _bufferStream.Read(remainingBuffer, 0, remainingBuffer.Length);

            _bufferStream.SetLength(0);
            _bufferStream.Write(remainingBuffer, 0, remainingBuffer.Length);

            return message;
        }

        public void Dispose()
        {
            _bufferStream.Dispose();
        }
    }
}