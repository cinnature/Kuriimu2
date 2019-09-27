﻿using System;
using System.IO;

namespace Kompression.IO
{
    class CircularBuffer : IDisposable
    {
        private byte[] _buffer;

        public int Length => _buffer.Length;

        public int Position { get; set; }

        public CircularBuffer(int bufferSize)
        {
            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));

            _buffer = new byte[bufferSize];
        }

        public int ReadByte()
        {
            var value = _buffer[Position++ % Length];
            return value;
        }

        public void Read(byte[] buffer, int offset, int length)
        {
            while (length > 0)
            {
                var toRead = Math.Min(Length - Position % Length, length);
                Array.Copy(_buffer, Position % Length, buffer, offset, toRead);

                Position += toRead;
                offset += toRead;
                length -= toRead;
            }
        }

        public void WriteByte(byte value)
        {
            _buffer[Position++ % Length] = value;
        }

        public void Write(byte[] buffer, int offset, int length)
        {
            while (length > 0)
            {
                var toWrite = Math.Min(Length - Position % Length, length);
                Array.Copy(buffer, offset, _buffer, Position % Length, toWrite);

                Position += toWrite;
                offset += toWrite;
                length -= toWrite;
            }
        }

        public void Dispose()
        {
            Array.Clear(_buffer, 0, _buffer.Length);
            _buffer = null;
        }

        public void Copy(Stream output, int displacement, int length)
        {
            var displacedPosition = Position - displacement;
            var outputPosition = Position;

            var absoluteDisplacement = Math.Abs(displacement);
            var buffer = new byte[absoluteDisplacement];
            for (int i = 0; i < length; i += absoluteDisplacement)
            {
                var toCopy = Math.Min(absoluteDisplacement, length - i);

                Position = displacedPosition;
                Read(buffer, 0, toCopy);

                output.Write(buffer, 0, toCopy);

                Position = outputPosition;
                Write(buffer, 0, toCopy);

                displacedPosition += toCopy;
                outputPosition += toCopy;
            }
        }
    }
}
