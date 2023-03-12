using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NoitaSeedTool
{
    class Program
    {
        static int lastSeed = 0;
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        static void Main(string[] args)
        {
            while (true)
            {
                int currentSeed = GetWorldSeed();

                if (currentSeed != lastSeed)
                {
                    lastSeed = currentSeed;

                    string url = "https://noitool.com/?seed=" + currentSeed;
                    Console.WriteLine(currentSeed);
                    OpenWebPage(url);
                    Thread.Sleep(450);
                    Process[] processes = Process.GetProcessesByName("noita");
                    if (processes.Length > 0)
                    {
                        // Found the process, focus the window
                        IntPtr windowHandle = processes[0].MainWindowHandle;
                        SetForegroundWindow(windowHandle);
                    }
                    else
                    {
                        // No process found
                        Console.WriteLine("Noita is not running.");
                    }
                }

                Thread.Sleep(1000);
            }
        }

        static int GetWorldSeed()
        {
            int seed = 0;
            Process noitaProcess = Process.GetProcessesByName("noita").FirstOrDefault();

            if (noitaProcess != null)
            {
                IntPtr moduleBase = noitaProcess.MainModule.BaseAddress;
                IntPtr seedAddress = moduleBase + 0xBF5A48;

                byte[] seedBytes = new byte[4];
                int bytesRead = 0;
                bool success = ReadProcessMemory(noitaProcess.Handle, seedAddress, seedBytes, seedBytes.Length, ref bytesRead);


                if (success && bytesRead == seedBytes.Length)
                {
                    seed = BitConverter.ToInt32(seedBytes, 0);
                }
            }

            return seed;
        }

        static void OpenWebPage(string url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);
    }
}