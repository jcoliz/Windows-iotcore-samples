using System;
using System.Runtime.InteropServices;

namespace MemoryStatusCS
{
    class Program
    {
        // P/Invoke the GlobalMemoryStatus API.
        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, EntryPoint = "GlobalMemoryStatusEx", SetLastError = true)]
        static extern bool GlobalMemoryStatusEx( [In,Out] MEMORYSTATUSEX lpBuffer);

        public const string Name = "MemoryStatusCS";

        private const int KB = 1024;

        static int Main(string[] args)
        {
            bool success = false;

            MEMORYSTATUSEX lpBuffer = new MEMORYSTATUSEX();
            var result = GlobalMemoryStatusEx(lpBuffer);

            if (!result)
            {
                Console.Error.WriteLine("Failed getting memory status");
            }
            else
            {
                Console.WriteLine($"Memory in use {lpBuffer.dwMemoryLoad}%");
                Console.WriteLine($"Total size of physical memory: {lpBuffer.ullTotalPhys / KB}KB");
                Console.WriteLine($"Size of physical memory available: {lpBuffer.ullAvailPhys / KB}KB");
                Console.WriteLine($"Size of the committed memory limit: {lpBuffer.ullTotalPageFile / KB}KB");
                Console.WriteLine($"Size of available memory to commit: {lpBuffer.ullAvailPageFile / KB}KB");
                Console.WriteLine($"Total size of the user mode portion of the virtual address space of the calling process: {lpBuffer.ullTotalVirtual / KB}KB");
                Console.WriteLine($"Size of unreserved and uncommitted memory in the user mode portion of the virtual address space of the calling process: {lpBuffer.ullAvailVirtual / KB}KB");
                Console.WriteLine($"Size of unreserved and uncommitted memory in the extended portion of the virtual address space of the calling process: {lpBuffer.ullAvailExtendedVirtual / KB}KB");

                success = true;
            }

            return success ? 0 : 1;
        }
    }
}
