using DuplicatesFinder.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicatesFinder.MainLogic
{
    public class EqualFileGroup
    {
        public EqualFileGroup()
        {
            Files = new List<FileData>(4);
        }
        public EqualFileGroup(FileData initial)
        {
            Files = new List<FileData>(4);
            Files.Add(initial);
        }

        public List<FileData> Files { get; private set; }

        public FileData Initial
        {
            get
            {
                if (Files.Count == 0)
                    return null;
                return Files[0];
            }
        }
    }
}
