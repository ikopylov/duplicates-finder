using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicatesFinder.MainLogic
{
    public struct FastFileKey : IEquatable<FastFileKey>
    {
        private long _fileSize;

        public FastFileKey(long size)
        {
            _fileSize = size;
        }


        public override int GetHashCode()
        {
            return _fileSize.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(obj, null))
                return false;

            if (!(obj is FastFileKey))
                return false;

            FastFileKey other = (FastFileKey)obj;

            return _fileSize == other._fileSize;
        }


        public bool Equals(FastFileKey other)
        {
            return _fileSize == other._fileSize;
        }
    }
}
