using System.Linq;
using System.Windows.Forms;
using MenuSystem;
using Service;

namespace Ui {
    public static class Program {
        private static readonly TrayAppContext TrayAppContext = new TrayAppContext();
        
        /// <summary>
        /// The main entry point for the UI application.
        /// </summary>
        public static void Main() {
            HookMenuSystem();
            Config.LoadConfig();

            try {
                Application.Run(TrayAppContext);
            } finally {
                Service.Service.Stop();
                ConsoleManager.Deallocate();
            }
        }

        /// <summary>
        /// Hook functionality to the menu system
        /// </summary>
        private static void HookMenuSystem() {
            Menus.SessIdInputMenu.InputPropagateFunc = Config.SessIdInputPropagate;
            Menus.AccountNameInputMenu.InputPropagateFunc = Config.AccountNameInputPropagate;
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
            
            // Give LogParser access to show warning messages. Bit spaghetti but it'll do for now.
            LogParser.TooltipMsg = TrayAppContext.TooltipMsg;
        }
    }
}