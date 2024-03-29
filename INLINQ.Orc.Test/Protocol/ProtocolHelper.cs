﻿using INLINQ.Orc.Test.TestHelpers;
using System;
using System.IO;

namespace INLINQ.Orc.Test.Protocol
{
    public class ProtocolHelper : IDisposable
    {
        private readonly DataFileHelper _dataFileHelper;

        public ProtocolHelper(string dataFileName)
        {
            _dataFileHelper = new DataFileHelper(dataFileName);
        }

        public ProtocolHelper(Stream inputStream)
        {
            _dataFileHelper = new DataFileHelper(inputStream);
        }

        public int GetPostscriptLength()
        {
            byte lastByte = _dataFileHelper.Read(_dataFileHelper.Length - 1, 1)[0];
            return lastByte;
        }

        public Stream GetPostscriptStream(int postscriptLength)
        {
            long offset = _dataFileHelper.Length - 1 - postscriptLength;
            return _dataFileHelper.GetStreamSegment(offset, (ulong)postscriptLength);
        }

        public Stream GetFooterCompressedStream(int postscriptLength, ulong footerLength)
        {
            long offset = _dataFileHelper.Length - 1 - postscriptLength - (long)footerLength;
            return _dataFileHelper.GetStreamSegment(offset, footerLength);
        }

        public Stream GetMetadataCompressedStream(int postscriptLength, ulong footerLength, ulong metadataLength)
        {
            long offset = _dataFileHelper.Length - 1 - postscriptLength - (long)footerLength - (long)metadataLength;
            return _dataFileHelper.GetStreamSegment(offset, metadataLength);
        }

        public Stream GetStripeFooterCompressedStream(ulong stripeOffset, ulong indexLength, ulong dataLength, ulong footerLength)
        {
            ulong offset = stripeOffset + indexLength + dataLength;
            return _dataFileHelper.GetStreamSegment((long)offset, footerLength);
        }

        public Stream GetRowIndexCompressedStream(ulong offset, ulong length)
        {
            return _dataFileHelper.GetStreamSegment((long)offset, length);
        }

        public Stream GetDataCompressedStream(ulong offset, ulong length)
        {
            return _dataFileHelper.GetStreamSegment((long)offset, length);
        }

        public void Dispose()
        {
            _dataFileHelper.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
