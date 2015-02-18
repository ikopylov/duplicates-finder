using DuplicatesFinder.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicatesFinder.FileComparison
{
    public class RealFileSizeComparer: FileComparer
    {
        public override FileComparsionResult IsEqual(FileData f1, FileData f2)
        {
            if (f1.RealFileLength != f2.RealFileLength)
                return FileComparsionResult.NotEqual;

            return FileComparsionResult.NeedAdditionalCheck;
        }
    }
}
