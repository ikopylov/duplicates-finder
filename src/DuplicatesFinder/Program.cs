using DuplicatesFinder.FileComparison;
using DuplicatesFinder.Helpers;
using DuplicatesFinder.MainLogic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicatesFinder
{
    class Program
    {
        enum WorkMode
        {
            List,
            Delete,
            HardLink
        }

        static void PrintHelp()
        {
            Console.WriteLine(@"DuplicatesFinder \mode:<mode> [\includeEmpty] [\includeSoftLinks] [\bufSize:268435456] <file> <scanDirectory>");
            Console.WriteLine("mode: list, delete, hardLink");
            Console.WriteLine();
        }

        static void RunTest(string input, string expected)
        {
            var encs = Encoding.GetEncodings();
            foreach (var src in encs)
            {
                foreach (var target in encs)
                {
                    var bt = src.GetEncoding().GetBytes(input);
                    var str = target.GetEncoding().GetString(bt);

                    if (str == expected)
                    {
                        Console.WriteLine("source: " + src.DisplayName + ", target = " + target.DisplayName);
                        return;
                    }
                }
            }
        }


        private static string DecodeString(string src)
        {
            if (System.Console.InputEncoding == Encoding.Default)
                return src;

            var bytes = System.Console.InputEncoding.GetBytes(src);
            return Encoding.Default.GetString(bytes);
        }

        private static bool IsFileNameValid(string fileName)
        {
            try
            {
                var fi = new Alphaleonis.Win32.Filesystem.FileInfo(fileName);
                return fi != null;
            }
            catch (Exception) { }

            return false;
        }

        private static bool IsDirectoryNameValid(string dirName)
        {
            try
            {
                var di = new Alphaleonis.Win32.Filesystem.DirectoryInfo(dirName);
                return di != null;
            }
            catch (Exception) { }

            return false;
        }


        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                PrintHelp();
                return;
            }

            WorkMode mode = WorkMode.List;
            bool withEmpty = false;
            bool withSoftLinks = false;
            int bufferSize = -1;
            string targetFile = null;
            string scanDir = null;
            string pattern = null;

            for (int i = 0; i < args.Length - 2; i++)
            {
                string curArg = args[i];
                if (!curArg.StartsWith(@"\"))
                {
                    Console.WriteLine("Bad argument: " + args[i]);
                    return;
                }
                curArg = curArg.Substring(1);

                string[] parts = curArg.Split(':');
                if (parts.Length > 2)
                {
                    Console.WriteLine("Bad argument: " + args[i]);
                    return;
                }

                switch (parts[0].ToLower())
                {
                    case "mode":
                        if (parts.Length != 2)
                        {
                            Console.WriteLine("Bad argument: " + args[i]);
                            return;
                        }

                        switch (parts[1].ToLower())
                        {
                            case "list":
                                mode = WorkMode.List;
                                break;
                            case "delete":
                                mode = WorkMode.Delete;
                                break;
                            case "hardlink":
                                mode = WorkMode.HardLink;
                                break;
                            default:
                                Console.WriteLine("Bad argument: " + args[i]);
                                return;
                        }

                        break;
                    case "includeempty":
                        withEmpty = true;
                        break;
                    case "includesoftlinks":
                        withSoftLinks = true;
                        break;
                    case "bufsize":
                        if (parts.Length != 2)
                        {
                            Console.WriteLine("Bad argument: " + args[i]);
                            return;
                        }
                        if (!int.TryParse(parts[1], out bufferSize))
                        {
                            Console.WriteLine("Bad argument: " + args[i]);
                            return;
                        }
                        break;
                    case "pattern":
                        if (parts.Length != 2)
                        {
                            Console.WriteLine("Bad argument: " + args[i]);
                            return;
                        }
                        pattern = parts[1];
                        break;
                    default:
                        Console.WriteLine("Bad argument: " + args[i]);
                        return;
                }
            }

            targetFile = DecodeString(args[args.Length - 2]);
            scanDir = DecodeString(args[args.Length - 1]);

            bool wasDirReverted = false;
            if (!IsDirectoryNameValid(scanDir) || !Directory.Exists(scanDir))
            {
                var origScanDir = args[args.Length - 1];
                if (IsDirectoryNameValid(origScanDir) && Directory.Exists(origScanDir))
                {
                    wasDirReverted = true;
                    scanDir = origScanDir;
                }
            }
            if (!IsFileNameValid(targetFile) || wasDirReverted)
            {
                var origTargetFile = args[args.Length - 2];
                if (IsFileNameValid(origTargetFile))
                    targetFile = origTargetFile;
            }

            if (!Directory.Exists(scanDir))
            {
                Console.WriteLine("Scan directory not exists: " + scanDir);
                return;
            }



            try
            {
                ChainComparer comparer = new ChainComparer();

                if (withSoftLinks)
                    comparer.Chain.Add(new RealFileSizeComparer());
                else
                    comparer.Chain.Add(new FileSizeComparer());

                comparer.Chain.Add(new HardLinkComparer());
                comparer.Chain.Add(new FastFileContentComparer());

                if (bufferSize <= 0)
                    comparer.Chain.Add(new FullFileContentComparer());
                else
                    comparer.Chain.Add(new FullFileContentComparer(bufferSize));



                var eqFinder = new EqualFilesFinder(scanDir, comparer, !withSoftLinks, pattern);
                var resList = eqFinder.Find();
                resList.Sort((a, b) => a.Initial.FullName.CompareTo(b.Initial.FullName));

                ActionBuilder.PrintStatistic(resList);

                switch (mode)
                {
                    case WorkMode.List:
                        ActionBuilder.BuildDuplicatesList(targetFile, resList, !withEmpty);
                        break;
                    case WorkMode.Delete:
                        ActionBuilder.BuildDeleteList(targetFile, resList, !withEmpty);
                        break;
                    case WorkMode.HardLink:
                        ActionBuilder.BuildHardLinkList(targetFile, resList, !withEmpty);
                        break;
                    default:
                        throw new InvalidOperationException("Unknown mode: " + mode.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unexpected error: " + ex.ToString());
            }

            Console.WriteLine("Finished. Data in file: " + targetFile);
            Console.ReadLine();
        }
    }
}
