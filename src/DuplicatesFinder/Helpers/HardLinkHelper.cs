using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using IO = Alphaleonis.Win32.Filesystem;

namespace DuplicatesFinder.Helpers
{
    public static class HardLinkHelper
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct BY_HANDLE_FILE_INFORMATION
        {
            public uint FileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME CreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastWriteTime;
            public uint VolumeSerialNumber;
            public uint FileSizeHigh;
            public uint FileSizeLow;
            public uint NumberOfLinks;
            public uint FileIndexHigh;
            public uint FileIndexLow;
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern SafeFileHandle CreateFile(
            string lpFileName,
            [MarshalAs(UnmanagedType.U4)] FileAccess dwDesiredAccess,
            [MarshalAs(UnmanagedType.U4)] FileShare dwShareMode,
            IntPtr lpSecurityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode dwCreationDisposition,
            [MarshalAs(UnmanagedType.U4)] FileAttributes dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetFileInformationByHandle(SafeFileHandle handle, out BY_HANDLE_FILE_INFORMATION lpFileInformation);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern IntPtr FindFirstFileNameW(
            string lpFileName,
            uint dwFlags,
            ref uint stringLength,
            StringBuilder fileName);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool FindNextFileNameW(
            IntPtr hFindStream,
            ref uint stringLength,
            StringBuilder fileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FindClose(IntPtr fFindHandle);

        [DllImport("kernel32.dll")]
        static extern bool GetVolumePathName(string lpszFileName,
            [Out] StringBuilder lpszVolumePathName, uint cchBufferLength);

        [DllImport("shlwapi.dll", CharSet = CharSet.Auto)]
        static extern bool PathAppend([In, Out] StringBuilder pszPath, string pszMore);


        public static int GetHardLinkCount(string filepath)
        {
            int result = 0;
            SafeFileHandle handle = null;
            try
            {
                handle = CreateFile(filepath, FileAccess.Read, FileShare.Read, IntPtr.Zero, FileMode.Open, FileAttributes.Archive, IntPtr.Zero);
                BY_HANDLE_FILE_INFORMATION fileInfo = new BY_HANDLE_FILE_INFORMATION();
                if (GetFileInformationByHandle(handle, out fileInfo))
                    result = (int)fileInfo.NumberOfLinks;
            }
            finally
            {
                if (handle != null)
                    handle.Close();
            }
            return result;
        }


        public static int GetHardLinkCount(this FileInfo file)
        {
            return GetHardLinkCount(file.FullName);
        }



        public static string[] GetFileSiblingHardLinks(string filepath)
        {
            const int MaxFilePath = 16535;
            StringBuilder sb = new StringBuilder(MaxFilePath);
            uint stringLength = MaxFilePath;

            List<string> result = new List<string>();

            GetVolumePathName(filepath, sb, stringLength);
            string volume = sb.ToString();

            sb.Length = 0;
            stringLength = MaxFilePath;
            IntPtr findHandle = FindFirstFileNameW(filepath, 0, ref stringLength, sb);

            if (findHandle.ToInt32() == -1)
                return null;

            try
            {
                do
                {
                    result.Add(IO.Path.Combine(volume, sb.ToString()));
                }
                while (FindNextFileNameW(findHandle, ref stringLength, sb));
            }
            finally
            {
                FindClose(findHandle);
            }

            return result.ToArray();
        }

        public static FileInfo[] GetHardLinks(this FileInfo file)
        {
            var resTmp = GetFileSiblingHardLinks(file.FullName);

            return resTmp.Select(o => new FileInfo(o)).ToArray();
        }
    }
}
