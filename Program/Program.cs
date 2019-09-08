using System.Windows.Forms;

namespace Program {
    internal static class Program {
        /// <summary>
        /// Entry point for the application
        /// </summary>
        public static void Main(string[] args) {
            // We use Windows Forms to run the application as a tray app
            Application.Run(new TrayAppContext());
        }
    }
}