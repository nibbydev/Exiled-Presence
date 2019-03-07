using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Utility {
    public static class Win32 {
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


            return gameProc.MainModule.FileName;
        }
    }
}