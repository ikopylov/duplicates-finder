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
                    default:
                        Console.WriteLine("Bad argument: " + args[i]);
                        return;
                }
            }

            targetFile = args[args.Length - 2];
            scanDir = args[args.Length - 1];

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



                var eqFinder = new EqualFilesFinder(scanDir, comparer, !withSoftLinks);
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
