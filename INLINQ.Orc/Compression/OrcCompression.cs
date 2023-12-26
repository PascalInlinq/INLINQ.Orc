using INLINQ.Orc.Encodings;
using INLINQ.Orc.Infrastructure;
using INLINQ.Orc.Protocol;
using System.Diagnostics;

namespace INLINQ.Orc.Compression
{
    using IOStream = System.IO.Stream;

    public static class OrcCompression
    {
        //public static long copyToTotalMilliSeconds { get; private set; }

        /// <summary>
        /// Provides a Stream that when read from, reads consecutive blocks of compressed data from an ORC Stream.
        /// All data in the <paramref name="inputStream"/> will be consumed.
        /// </summary>
        public static ConcatenatingStream GetDecompressingStream(IOStream inputStream, CompressionKind compressionKind)
        {
            if (compressionKind == CompressionKind.None)
            {
                return new ConcatenatingStream(inputStream, false);
            }
            else
            {
                return new ConcatenatingStream(() =>
                {
                    bool headerAvailable = ReadBlockHeader(inputStream, out int blockLength, out bool isCompressed);
                    if (!headerAvailable)
                    {
                        return null;
                    }

                    StreamSegment? streamSegment = new(inputStream, blockLength, true);

                    if (!isCompressed)
                    {
                        return streamSegment;
                    }
                    else
                    {
                        return CompressionFactory.CreateDecompressorStream(compressionKind, streamSegment);
                    }
                }, false);
            }
        }

        public static byte[] GetDecompressingStreamBytes(IOStream inputStream, CompressionKind compressionKind)
        {
            if (compressionKind == CompressionKind.None)
            {
                long length = inputStream.Length;
                byte[] result = new byte[length];
                inputStream.Write(result, 0, (int)length);
                return result;
            }
            else
            {
                bool headerAvailable = ReadBlockHeader(inputStream, out int blockLength, out bool isCompressed);
                if (!headerAvailable)
                {
                    return Array.Empty<byte>();
                }
                else
                {
                    if (!isCompressed)
                    {
                        byte[] result = new byte[blockLength];
                        inputStream.Write(result, 0, blockLength);
                        return result;
                    }
                    else
                    {
                        StreamSegment? streamSegment = new(inputStream, blockLength, true);
                        IOStream io = CompressionFactory.CreateDecompressorStream(compressionKind, streamSegment);
                        long length = io.Length;
                        byte[] result = new byte[length];
                        inputStream.Write(result, 0, (int)length);
                        return result;
                    }
                }
            }
        }



        /// <summary>
        /// Compress the entirety of the MemoryStream to the destination Stream, or if compressing increases the size, just copy the original data over
        /// </summary>
        /// <param name="uncompressedSource">A MemoryStream that will have its full contents compressed</param>
        /// <param name="compressedDestination">Output Stream to write results to</param>
        /// <param name="compressionKind">Compression Type</param>
        /// <param name="compressionStrategy">The balance of speed vs size for the compressor</param>
        public static void CompressCopyTo(MemoryStream uncompressedSource, IOStream compressedDestination, CompressionKind compressionKind, CompressionStrategy compressionStrategy)
        {
            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            _ = uncompressedSource.Seek(0, SeekOrigin.Begin);

            if (compressionKind == CompressionKind.None)
            {
                uncompressedSource.CopyTo(compressedDestination);
            }
            else
            {
                MemoryStream? temporaryStream = new();   //How can we avoid this?? We need to know the length of the compressed stream to write the header before we can write the stream itself...
                using (IOStream? compressingStream = CompressionFactory.CreateCompressorStream(compressionKind, compressionStrategy, temporaryStream))
                {
                    uncompressedSource.CopyTo(compressingStream);
                }

                if (temporaryStream.Length >= uncompressedSource.Length)
                {
                    //Write the original data rather than the compressed data
                    _ = uncompressedSource.Seek(0, SeekOrigin.Begin);
                    WriteBlockHeader(compressedDestination, (int)uncompressedSource.Length, false);
                    uncompressedSource.CopyTo(compressedDestination);
                }
                else
                {
                    _ = temporaryStream.Seek(0, SeekOrigin.Begin);
                    WriteBlockHeader(compressedDestination, (int)temporaryStream.Length, true);
                    temporaryStream.CopyTo(compressedDestination);
                }
            }

            //copyToTotalMilliSeconds += sw.ElapsedMilliseconds;
        }

        private static bool ReadBlockHeader(IOStream inputStream, out int blockLength, out bool isCompressed)
        {
            int firstByte = inputStream.ReadByte();
            if (firstByte < 0)      //End of stream
            {
                blockLength = 0;
                isCompressed = false;
                return false;
            }

            //From here it's a data error if the stream ends
            int rawValue = firstByte | inputStream.CheckedReadByte() << 8 | inputStream.CheckedReadByte() << 16;
            blockLength = rawValue >> 1;
            isCompressed = (rawValue & 1) == 0;

            return true;
        }

        private static void WriteBlockHeader(IOStream outputStream, int blockLength, bool isCompressed)
        {
            if (blockLength > 0x7FFFFF)
            {
                throw new OverflowException($"Compressed block size cannot be larger than 8*3-1 bits (8MB). Attempted to store {blockLength} bytes.");
            }

            int value = blockLength << 1;
            if (!isCompressed)
            {
                value |= 1;
            }

            byte[]? bytes = BitConverter.GetBytes(value);
            outputStream.WriteByte(bytes[0]);
            outputStream.WriteByte(bytes[1]);
            outputStream.WriteByte(bytes[2]);
        }

        public static CompressionKind ToCompressionKind(this CompressionType configurationCompressionType)
        {
            switch (configurationCompressionType)
            {
                case CompressionType.None: return CompressionKind.None;
                case CompressionType.ZLIB: return CompressionKind.Zlib;
                default:
                    throw new ArgumentException($"Unhandled {nameof(CompressionType)} {configurationCompressionType}");
            }
        }
    }
}
