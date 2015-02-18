using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IO = Alphaleonis.Win32.Filesystem;

namespace DuplicatesFinder.MainLogic
{
    public class ActionBuilder
    {
        private static string GetSizeString(long size)
        {
            if (size < 4 * 1024)
                return size.ToString() + "b";

            string rest = "";

            rest = (size % 1024).ToString() + "b" + rest;
            size = size / 1024;

            if (size < 4 * 1024)
                return size.ToString() + "kb " + rest;

            rest = (size % 1024).ToString() + "kb " + rest;
            size = size / 1024;

            if (size < 4 * 1024)
                return size.ToString() + "mb " + rest;

            rest = (size % 1024).ToString() + "mb " + rest;
            size = size / 1024;

            return size.ToString() + "gb " + rest;
        }


        public static void PrintStatistic(List<EqualFileGroup> groups)
        {
            Console.WriteLine("Stats:");
            Console.WriteLine("  Total files: " + groups.SelectMany(o => o.Files).Count().ToString());
            Console.WriteLine("  Total size on disk: " + GetSizeString(groups.SelectMany(o => o.Files).Select(o => o.FileLength).Sum()));
            Console.WriteLine("  Equal groups: " + groups.Count.ToString());
            Console.WriteLine("  Duplicate groups: " + groups.Where(o => o.Files.Count > 1).Count().ToString());
            Console.WriteLine("  Duplicates: " + groups.Where(o => o.Files.Count > 1).SelectMany(o => o.Files.Skip(1)).Count().ToString());
            Console.WriteLine("  Duplicates size on disk: " + GetSizeString(groups.Where(o => o.Files.Count > 1).SelectMany(o => o.Files.Skip(1)).Select(o => o.FileLength).Sum()));
            Console.WriteLine("  Empty files: " + groups.Where(o => o.Initial.FileLength == 0).Count());
            Console.WriteLine();
        }




        public static void BuildDuplicatesList(string targetFile, List<EqualFileGroup> groups, bool ignoreEmpty = true)
        {
            var dir = IO.Path.GetDirectoryName(targetFile);
            if (!IO.Directory.Exists(dir))
                IO.Directory.CreateDirectory(dir);

            using (var wrtFile = IO.File.CreateText(targetFile))
            {
                wrtFile.Flush();
                wrtFile.BaseStream.Position = 0;
                foreach (var group in groups)
                {
                    if (group.Files.Count <= 1)
                        continue;

                    if (ignoreEmpty && group.Initial.RealFileLength == 0)
                        continue;


                    foreach (var file in group.Files)
                        wrtFile.WriteLine(file.FullName);

                    wrtFile.WriteLine();
                }
            }
        }



        public static void BuildDeleteList(string targetFile, List<EqualFileGroup> groups, bool ignoreEmpty = true)
        {
            var dir = IO.Path.GetDirectoryName(targetFile);
            if (!IO.Directory.Exists(dir))
                IO.Directory.CreateDirectory(dir);

            using (var wrtFile = IO.File.CreateText(targetFile))
            {
                wrtFile.Flush();
                wrtFile.BaseStream.Position = 0;
                foreach (var group in groups)
                {
                    if (group.Files.Count <= 1)
                        continue;

                    if (ignoreEmpty && group.Initial.RealFileLength == 0)
                        continue;


                    foreach (var file in group.Files.Skip(1))
                        wrtFile.WriteLine("del \"" + file.FullName + "\"");

                    wrtFile.WriteLine();
                }

                wrtFile.WriteLine("pause");
            }
        }

        public static void BuildHardLinkList(string targetFile, List<EqualFileGroup> groups, bool ignoreEmpty = true)
        {
            var dir = IO.Path.GetDirectoryName(targetFile);
            if (!IO.Directory.Exists(dir))
                IO.Directory.CreateDirectory(dir);

            using (var wrtFile = IO.File.CreateText(targetFile))
            {
                wrtFile.Flush();
                wrtFile.BaseStream.Position = 0;
                foreach (var group in groups)
                {
                    if (group.Files.Count <= 1)
                        continue;

                    if (ignoreEmpty && group.Initial.RealFileLength == 0)
                        continue;


                    foreach (var file in group.Files.Skip(1))
                    {
                        wrtFile.WriteLine("del \"" + file.FullName + "\"");
                        wrtFile.WriteLine("mklink /H \"" + file.FullName + "\" \"" + group.Initial.FullName + "\"");

                        wrtFile.WriteLine();
                    }

                    wrtFile.WriteLine();
                }

                wrtFile.WriteLine("pause");
            }
        }
    }
}
