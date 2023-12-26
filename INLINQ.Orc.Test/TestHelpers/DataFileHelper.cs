using INLINQ.Orc.Infrastructure;
using Microsoft.Extensions.FileProviders;
using System.Reflection;

namespace INLINQ.Orc.Test.TestHelpers
{
    public class DataFileHelper : IDisposable
    {
        private readonly Stream _dataStream;
        public DataFileHelper(string dataFileName)
        {
            string embeddedFileName = $"Data{Path.DirectorySeparatorChar}{dataFileName}";

            EmbeddedFileProvider fileProvider = new(typeof(DataFileHelper).GetTypeInfo().Assembly);
            IFileInfo fileInfo = fileProvider.GetFileInfo(embeddedFileName);
            if (!fileInfo.Exists)
            {
                throw new ArgumentException("Requested data file doesn't exist");
            }

            _dataStream = fileInfo.CreateReadStream();
        }

        public DataFileHelper(Stream inputStream)
        {
            _dataStream = inputStream;
        }

        public void Dispose()
        {
            _dataStream.Dispose();
            GC.SuppressFinalize(this);
        }

        public long Length => _dataStream.Length;
        public byte[] Read(long fileOffset, int length)
        {
            byte[] buffer = new byte[length];
            _ = _dataStream.Seek(fileOffset, SeekOrigin.Begin);
            int readLen = _dataStream.Read(buffer, 0, length);
            return readLen != length ? throw new InvalidOperationException("Read returned less data than requested") : buffer;
        }

        public Stream GetStreamSegment(long fileOffset, ulong length)
        {
            _ = _dataStream.Seek(fileOffset, SeekOrigin.Begin);
            return new StreamSegment(_dataStream, (long)length, true);
        }

        public Stream GetStream()
        {
            return _dataStream;
        }
    }
}
