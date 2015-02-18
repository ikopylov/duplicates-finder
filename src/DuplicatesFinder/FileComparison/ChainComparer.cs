using DuplicatesFinder.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicatesFinder.FileComparison
{
    public class ChainComparer: FileComparer
    {
        public ChainComparer()
        {
            Chain = new List<FileComparer>();
        }
        public ChainComparer(IEnumerable<FileComparer> src)
        {
            Chain = new List<FileComparer>(src);
        }

        public List<FileComparer> Chain { get; private set; }


        public override FileComparsionResult IsEqual(FileData f1, FileData f2)
        {
            for (int i = 0; i < Chain.Count; i++)
            {
                var cmpRes = Chain[i].IsEqual(f1, f2);
                switch (cmpRes)
                {
                    case FileComparsionResult.Equal:
                        return FileComparsionResult.Equal;
                    case FileComparsionResult.NotEqual:
                        return FileComparsionResult.NotEqual;
                    case FileComparsionResult.NeedAdditionalCheck:
                        break;
                    default:
                        throw new InvalidOperationException("FileComparsionResult is unknown: " + cmpRes.ToString());
                }
            }

            return FileComparsionResult.NeedAdditionalCheck;
        }
    }
}
