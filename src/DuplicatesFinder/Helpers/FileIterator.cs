using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IO = Alphaleonis.Win32.Filesystem;

namespace DuplicatesFinder.Helpers
{
    public static class FileIterator
    {
        public static IEnumerable<string> GetFiles(string path)
        {
            Stack<string> queue = new Stack<string>();
            queue.Push(path);
            while (queue.Count > 0)
            {
                path = queue.Pop();
                try
                {
                    var dirs = IO.Directory.GetDirectories(path);
                    for (int i = dirs.Length - 1; i >= 0; i--)
                    {
                        queue.Push(dirs[i]);
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    Console.WriteLine("Unauthorized Access: " + ex.Message);
                }
                string[] files = null;
                try
                {
                    files = IO.Directory.GetFiles(path);
                }
                catch (UnauthorizedAccessException ex)
                {
                    Console.WriteLine("Unauthorized Access: " + ex.Message);
                }
                if (files != null)
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        yield return files[i];
                    }
                }
            }
        }
    }
}
