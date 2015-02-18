using DuplicatesFinder.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicatesFinder.FileComparison
{
    public class FullFileContentComparer: FileComparer
    {
        private int _bufferSize = 256 * 1024 * 1024;

        private BlockingCollection<byte[]> _bufferCache = new BlockingCollection<byte[]>();

        public FullFileContentComparer()
        {
            _bufferCache.Add(new byte[_bufferSize]);
            _bufferCache.Add(new byte[_bufferSize]);
        }
        public FullFileContentComparer(int bufferSize)
        {
            _bufferSize = bufferSize;
            _bufferCache.Add(new byte[_bufferSize]);
            _bufferCache.Add(new byte[_bufferSize]);
        }


        private byte[] GetBuffer()
        {
            return _bufferCache.Take();
        }
        private void ReleaseBuffer(byte[] buf)
        {
            _bufferCache.Add(buf);
        }



        private int GuarantyRead(FileStream stream, byte[] buffer)
        {
            int lastRead = 0;
            int totalReaded = 0;

            do
            {
                lastRead = stream.Read(buffer, totalReaded, buffer.Length - totalReaded);
                totalReaded += lastRead;
            }
            while (lastRead > 0 && totalReaded < buffer.Length);

            return totalReaded;
        }


        public override FileComparsionResult IsEqual(FileData f1, FileData f2)
        {
            using (var stream1 = f1.OpenRead())
            {
                using (var stream2 = f2.OpenRead())
                {
                    byte[] buffer1 = null, buffer2 = null;
                    try
                    {
                        buffer1 = GetBuffer();
                        buffer2 = GetBuffer();
                        int readed1 = 0;
                        int readed2 = 0;

                        do
                        {
                            readed1 = GuarantyRead(stream1, buffer1);
                            readed2 = GuarantyRead(stream2, buffer2);

                            if (readed1 != readed2)
                                return FileComparsionResult.NotEqual;

                            for (int i = 0; i < readed1; i++)
                                if (buffer1[i] != buffer2[i])
                                    return FileComparsionResult.NotEqual;
                        }
                        while (readed1 > 0 && readed2 > 0);
                    }
                    finally
                    {
                        if (buffer1 != null)
                            ReleaseBuffer(buffer1);
                        if (buffer2 != null)
                            ReleaseBuffer(buffer2);
                    }
                }
            }

            return FileComparsionResult.Equal;
        }
    }
}
