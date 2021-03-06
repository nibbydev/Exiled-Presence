using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Service {
    public static class Win32 {
        [DllImport("Kernel32.dll")]
        private static extern bool QueryFullProcessImageName([In] IntPtr hProcess, [In] uint dwFlags,
            [Out] StringBuilder lpExeName, [In, Out] ref uint lpdwSize);

        /// <summary>
        /// Check if a process with specified title is currently running
        /// </summary>
        public static bool IsRunning(string windowTitle) {
            if (string.IsNullOrEmpty(windowTitle)) {
                throw new ArgumentException("Invalid params provided");
            }

            var gameProc = Process.GetProcesses()
                .FirstOrDefault(t => t.MainWindowTitle.Equals(windowTitle));

            return gameProc != null && !gameProc.HasExited;
        }

        /// <summary>
        /// Finds the full path of the executable of the first process with the provided name
        /// </summary>
        public static string FindProcessPath(string windowTitle) {
            if (string.IsNullOrEmpty(windowTitle)) {
                throw new ArgumentException("Invalid params provided");
            }

            var gameProc = Process.GetProcesses()
                .FirstOrDefault(t => t.MainWindowTitle.Equals(windowTitle));

            if (gameProc == null || gameProc.HasExited) {
                return null;
            }

            return GetMainModuleFileName(gameProc);
        }

        /// <summary>
        /// Finds the executable path of a Process
        /// </summary>
        private static string GetMainModuleFileName(Process process, int buffer = 1024) {
            var fileNameBuilder = new StringBuilder(buffer);
            var bufferLength = (uint) fileNameBuilder.Capacity + 1;

            try {
                return QueryFullProcessImageName(process.Handle, 0, fileNameBuilder, ref bufferLength)
                    ? fileNameBuilder.ToString()
                    : null;
            } catch {
                return null;
            }
        }

        /// <summary>
        /// Creates a shortcut of the current application in the specified folder
        /// </summary>
        public static void CreateShortcut(string currentFile, string targetShortcut, string param = null) {
            if (!File.Exists(currentFile)) throw new FileNotFoundException();
            if (!Directory.GetParent(targetShortcut).Exists) throw new DirectoryNotFoundException();

            using (var sw = new StreamWriter(targetShortcut)) {
                sw.WriteLine("[InternetShortcut]");
                sw.WriteLine("URL=file:///" + currentFile + (param ?? ""));
                sw.WriteLine("IconIndex=0");
                sw.WriteLine("IconFile=" + currentFile);
                sw.Flush();
            }
        }
    }
}