﻿using System.IO;
using Kompression.Exceptions;
using Kompression.Implementations.Decoders.Headerless;
using Kompression.IO;
using Kontract.Kompression.Configuration;

namespace Kompression.Implementations.Decoders.Level5
{
    // First found in Inazuma Eleven 3: Ogre team as an ARCV compression
    public class InazumaLzssDecoder : IDecoder
    {
        private Lzss01HeaderlessDecoder _decoder;

        public InazumaLzssDecoder(int preBufferSize)
        {
            _decoder = new Lzss01HeaderlessDecoder(preBufferSize);
        }

        public void Decode(Stream input, Stream output)
        {
            var compressionHeader = new byte[4];
            input.Read(compressionHeader, 0, 4);
            if (compressionHeader[0] != 0x53 ||
                compressionHeader[1] != 0x53 ||
                compressionHeader[2] != 0x5A ||
                compressionHeader[3] != 0x4C)   // "SSZL"
                throw new InvalidCompressionException("Lzss");

            input.Position = 0xC;
            var decompressedSizeBuffer = new byte[4];
            input.Read(decompressedSizeBuffer, 0, 4);
            var decompressedSize = decompressedSizeBuffer[0] |
                                   (decompressedSizeBuffer[1] << 8) |
                                   (decompressedSizeBuffer[2] << 16) |
                                   (decompressedSizeBuffer[3] << 24);

            _decoder.Decode(input, output, decompressedSize);
        }

        public void Dispose()
        {
        }
    }
}
