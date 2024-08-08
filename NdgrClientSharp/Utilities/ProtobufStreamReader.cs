using System;
using System.IO;

namespace NdgrClientSharp.Utilities
{
    // このコードを参考にC#用に実装したもの
    // https://github.com/rinsuki-lab/ndgr-reader/blob/main/src/protobuf-stream-reader.ts
    internal sealed class ProtobufStreamReader : IDisposable
    {
        private readonly MemoryStream _bufferStream = new MemoryStream();

        public void AddNewChunk(byte[] chunk)
        {
            _bufferStream.Write(chunk, 0, chunk.Length);
        }

        private (int offset, int result)? ReadVarint()
        {
            var offset = 0;
            var result = 0;
            var shift = 0;

            _bufferStream.Position = 0;
            int b;

            while ((b = _bufferStream.ReadByte()) != -1)
            {
                result |= (b & 0x7F) << shift;
                offset++;
                shift += 7;

                if ((b & 0x80) == 0)
                {
                    return (offset, result);
                }
            }

            return null;
        }

        public byte[]? UnshiftChunk()
        {
            var varintResult = ReadVarint();
            if (varintResult == null) return null;

            var (offset, varint) = varintResult.Value;
            if (offset + varint > _bufferStream.Length)
            {
                return null;
            }

            var message = new byte[varint];

            // Read the message bytes directly into the array
            _bufferStream.Position = offset;
            _bufferStream.Read(message, 0, varint);

            // Shift the buffer content
            var remainingBuffer = new byte[_bufferStream.Length - offset - varint];
            _bufferStream.Position = offset + varint;
            _bufferStream.Read(remainingBuffer, 0, remainingBuffer.Length);

            // Reset and refill the stream with the remaining data
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
