using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
namespace Injection
{
    class Program
    {
        const int PROCESS_WM_READ = 0x0010;

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess,
        long lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        static void Main(string[] args)
        {
            Process process = Process.GetProcessesByName("javaw")[0];
            IntPtr processHandle = OpenProcess(PROCESS_WM_READ, false, process.Id);

            int bytesRead1 = 0;
            int bytesRead2 = 0;

            byte[] buffer = new byte[4]; //'Hello World!' takes 12*2 bytes because of Unicode 


            // 0x0046A3B8 is the address where I found the string, replace it with what you found
            byte[] pointBuff = new byte[4];

            ReadProcessMemory((int)processHandle, 0x8C3A5EE8, pointBuff, pointBuff.Length, ref bytesRead1);
            pointBuff[0] += 0x40;


            while (true) {
                ReadProcessMemory((int)processHandle, pointBuff[0], buffer, buffer.Length, ref bytesRead2);

                Console.WriteLine(buffer[0] +
               " (" + bytesRead2.ToString() + "bytes)");
                Thread.Sleep(10);
                
            }
            
        }
    }
}
