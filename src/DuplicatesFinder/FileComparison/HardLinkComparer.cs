using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DuplicatesFinder.Helpers;
using DuplicatesFinder.Common;

namespace DuplicatesFinder.FileComparison
{
    public class HardLinkComparer : FileComparer
    {
        public override FileComparsionResult IsEqual(FileData f1, FileData f2)
        {
            if (f1.HardLinkCount <= 1)
                return FileComparsionResult.NeedAdditionalCheck;

            if (f1.IsHardLinked(f2))
                return FileComparsionResult.Equal;

            return FileComparsionResult.NeedAdditionalCheck;
        }
    }
}
