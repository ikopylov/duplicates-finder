using DuplicatesFinder.Common;
using DuplicatesFinder.FileComparison;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IO = Alphaleonis.Win32.Filesystem;

namespace DuplicatesFinder.MainLogic
{
    public class EqualFilesFinder
    {
        private static readonly TimeSpan LogPeriod = TimeSpan.FromSeconds(30);

        private FileComparer _comparer;
        private bool _ignoreSoftLinks;
        private string _initialDir;

        public EqualFilesFinder(string dir, FileComparer comp, bool ignoreSoftLinks)
        {
            _initialDir = System.IO.Path.GetFullPath(dir);
            _comparer = comp;
            _ignoreSoftLinks = ignoreSoftLinks;
        }



        private FastFileKey GetKey(FileData f)
        {
            if (!_ignoreSoftLinks)
                return new FastFileKey(f.RealFileLength);

            return new FastFileKey(f.FileLength);
        }


        public List<EqualFileGroup> Find()
        {
            if (!IO.Directory.Exists(_initialDir))
            {
                Console.WriteLine("Directory is not exist: " + _initialDir);
                return new List<EqualFileGroup>();
            }

            long scannedFiles = 0;
            long duplicates = 0;
            DateTime startTime = DateTime.Now;
            DateTime lastTime = DateTime.Now;

            Dictionary<FastFileKey, List<EqualFileGroup>> fastDict = new Dictionary<FastFileKey, List<EqualFileGroup>>();

            foreach (var fileName in DuplicatesFinder.Helpers.FileIterator.GetFiles(_initialDir))
            {
                try
                {
                    var fData = new FileData(fileName);
                    var key = GetKey(fData);

                    scannedFiles++;

                    if (DateTime.Now - lastTime > LogPeriod)
                    {
                        Console.WriteLine(string.Format("Scanned {0} files. Current file: '{1}'", scannedFiles, fData.FullName));
                        Console.WriteLine();
                        lastTime = DateTime.Now;
                    }

                    List<EqualFileGroup> fastFiles = null;
                    if (!fastDict.TryGetValue(key, out fastFiles))
                    {
                        fastFiles = new List<EqualFileGroup>(4);
                        fastDict.Add(key, fastFiles);
                    }

                    bool groupFinded = false;

                    foreach (var curGroup in fastFiles)
                    {
                        var compRes = _comparer.IsEqual(fData, curGroup.Initial);
                        if (compRes == FileComparsionResult.NeedAdditionalCheck)
                            throw new InvalidOperationException("Bad FileComparsionResult: " + compRes);

                        if (_comparer.IsEqual(fData, curGroup.Initial) == FileComparsionResult.Equal)
                        {
                            duplicates++;
                            curGroup.Files.Add(fData);
                            groupFinded = true;
                            break;
                        }
                    }

                    if (!groupFinded)
                        fastFiles.Add(new EqualFileGroup(fData));
                }
                catch (System.IO.IOException ioExc)
                {
                    Console.WriteLine("IO error: " + ioExc.Message);
                }
                catch (UnauthorizedAccessException unEx)
                {
                    Console.WriteLine("Unauthorized Access error: " + unEx.Message);
                }
            }

            var result = fastDict.SelectMany(o => o.Value).ToList();

            Console.WriteLine(string.Format("Scan finished. Total files: {0}. Finded duplicates: {1}. Time: {2}", scannedFiles, duplicates, DateTime.Now - startTime));
            Console.WriteLine();

            return result;
        }
        
    }
}
