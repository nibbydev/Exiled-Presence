using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;

namespace Ui {
    [SuppressUnmanagedCodeSecurity]
    public static class ConsoleManager {
        private const int MF_BYCOMMAND = 0x00000000;
        private const int SC_CLOSE = 0xF060;

        [DllImport("User32.dll")]
        private static extern int ShowWindow(IntPtr intPtr, int nShow);

        [DllImport("user32.dll")]
        private static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();


        public static bool HasConsole => GetConsoleWindow() != IntPtr.Zero;
        public static bool IsConsoleVisible;

        /// <summary>
        /// Creates a new console instance if the process is not attached to a console already
        /// </summary>
        public static void Allocate() {
            if (!HasConsole) {
                AllocConsole();
                InvalidateOutAndError();
                IsConsoleVisible = true;
            }
        }

        /// <summary>
        /// Deallocates and removes the console
        /// </summary>
        public static void Deallocate() {
            if (HasConsole) {
                SetOutAndErrorNull();
                FreeConsole();
                IsConsoleVisible = false;
            }
        }

        public static void Show(object sender = null, EventArgs e = null) {
            if (!HasConsole) Allocate();

            var intPtr = GetConsoleWindow();
            ShowWindow(intPtr, 5);
            IsConsoleVisible = true;
        }

        public static void Hide(object sender = null, EventArgs e = null) {
            if (!HasConsole) return;

            var intPtr = GetConsoleWindow();
            ShowWindow(intPtr, 0);
            IsConsoleVisible = false;
        }

        private static void InvalidateOutAndError() {
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_CLOSE, MF_BYCOMMAND);

            const BindingFlags bindings = BindingFlags.Static | BindingFlags.NonPublic;
            var type = typeof(Console);

            var @out = type.GetField("_out", bindings);
            var error = type.GetField("_error", bindings);
            var initializeStdOutError = type.GetMethod("InitializeStdOutError", bindings);

            @out?.SetValue(null, null);
            error?.SetValue(null, null);

            initializeStdOutError?.Invoke(null, new object[] {true});
        }

        private static void SetOutAndErrorNull() {
            Console.SetOut(TextWriter.Null);
            Console.SetError(TextWriter.Null);
        }
    }
}