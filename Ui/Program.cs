using System.Linq;
using System.Windows.Forms;
using MenuSystem;
using Service;

namespace Ui {
    public static class Program {
        /// <summary>
        /// The main entry point for the UI application.
        /// </summary>
        public static void Main() {
            // Hook service to menus
            Menus.SessIdInputMenu.InputPropagateFunc = Service.Service.SessIdInputPropagate;
            Menus.AccountNameInputMenu.InputPropagateFunc = Service.Service.AccountNameInputPropagate;
            Menus.MainMenu.MenuItems.First(t => t.MenuToRun.Equals(Menus.AppStartFeedbackMenu)).ActionToExecute =
                Service.Service.Init;
            Menus.ConfigMenu.MenuItems.First(t => t.MenuToRun.Equals(Menus.ConfigClearedFeedbackMenu)).ActionToExecute =
                Config.ResetConfig;
            Menus.MainMenu.MenuItems.First(t => t.MenuToRun.Equals(Menus.AppStopFeedbackMenu)).ActionToExecute =
                Service.Service.Stop;

            // Hook config values to menus
            Menus.ConfigMenu.MenuItems.First(t => t.MenuToRun.Equals(Menus.AccountNameInputMenu)).ValueDelegate =
                () => Config.Settings.AccountName;
            Menus.ConfigMenu.MenuItems.First(t => t.MenuToRun.Equals(Menus.SessIdInputMenu)).ValueDelegate =
                Config.Settings.GetObfuscatedSessId;
            Menus.MainMenu.MenuItems.First(t => t.MenuToRun.Equals(Menus.AppStartFeedbackMenu)).ValueDelegate =
                () => Service.Service.IsRunning ? "Running" : null;

            // Load settings
            Config.LoadConfig();
            
            // Run the tray app
            try {
                Application.Run(new TrayAppContext());
            } finally {
                Service.Service.Stop();
                ConsoleManager.Deallocate();
            }
        }
    }
}