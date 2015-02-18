using DuplicatesFinder.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicatesFinder.FileComparison
{
    public enum FileComparsionResult
    {
        Equal,
        NotEqual,
        NeedAdditionalCheck
    }


    public abstract class FileComparer
    {
        public abstract FileComparsionResult IsEqual(FileData f1, FileData f2);
    }


}
