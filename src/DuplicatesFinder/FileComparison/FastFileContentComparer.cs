using DuplicatesFinder.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicatesFinder.FileComparison
{
    public class FastFileContentComparer : FileComparer
    {
        private readonly int _bufferSize = 1024 * 4;


        public FastFileContentComparer() { }
        public FastFileContentComparer(int bufferSize)
        {
            _bufferSize = bufferSize;
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

        private int ReadData(FileData f, byte[] buf)
        {
            using (var stream = f.OpenRead())
            {
                return GuarantyRead(stream, buf);
            }
        }

        public override FileComparsionResult IsEqual(FileData f1, FileData f2)
        {
            byte[] bufferF1 = new byte[_bufferSize];
            int realSizeF1 = ReadData(f1, bufferF1);

            byte[] bufferF2 = new byte[_bufferSize];
            int realSizeF2 = ReadData(f2, bufferF2);


            if (realSizeF1 != realSizeF2)
                return FileComparsionResult.NotEqual;

            for (int i = 0; i < realSizeF1; i++)
                if (bufferF1[i] != bufferF2[i])
                    return FileComparsionResult.NotEqual;


            if (f1.FileLength == realSizeF1 && f2.FileLength == realSizeF2)
                return FileComparsionResult.Equal;

            return FileComparsionResult.NeedAdditionalCheck;
        }
    }
}
