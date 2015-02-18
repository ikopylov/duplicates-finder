using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IO = Alphaleonis.Win32.Filesystem;

namespace DuplicatesFinder.Common
{
    public class FileData
    {
        private IO.FileInfo _fInf;
        private int _hardLinkCount = -1;
        private long _realFileSize = -1;

        public FileData(string path)
        {
            _fInf = new IO.FileInfo(path);
        }
        public FileData(System.IO.FileInfo fInf)
        {
            _fInf = new IO.FileInfo(fInf.FullName);
        }
        public FileData(IO.FileInfo fInf)
        {
            _fInf = fInf;
        }


        public long FileLength
        {
            get { return _fInf.Length; }
        }

        public long RealFileLength
        {
            get 
            {
                if (_realFileSize < 0)
                {
                    using (var tmp = _fInf.OpenRead())
                    {
                        _realFileSize = tmp.Length;
                    }
                }
                return _realFileSize; 
            }
        }

        public string FullName
        {
            get { return _fInf.FullName; }
        }

        public System.IO.FileStream OpenRead()
        {
            return _fInf.OpenRead();
        }

        public int HardLinkCount
        {
            get
            {
                if (_hardLinkCount < 0)
                    _hardLinkCount = DuplicatesFinder.Helpers.HardLinkHelper.GetHardLinkCount(_fInf.FullName);
                return _hardLinkCount;
            }
        }


        public string[] GetHardLinks()
        {
            return DuplicatesFinder.Helpers.HardLinkHelper.GetFileSiblingHardLinks(_fInf.FullName);
        }


        public bool IsHardLinked(FileData other)
        {
            var otherFullPath = IO.Path.GetRegularPath(other.FullName);
            var links = GetHardLinks();
            return links.Any(o => IO.Path.GetRegularPath(o) == otherFullPath);
        }


        public bool IsSoftLink
        {
            get
            {
                return _fInf.Attributes.HasFlag(System.IO.FileAttributes.ReparsePoint);
            }
        }
    }
}
