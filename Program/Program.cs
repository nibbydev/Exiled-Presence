using System.Linq;
using MenuSystem;
using Service;

namespace Program {
    internal static class Program {
        /// <summary>
        /// Entry point
        /// </summary>
        public static void Main(string[] args) {
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

            try {
                Menus.MainMenu.RunMenu();
            } finally {
                Service.Service.Stop();
            }
        }
    }
}