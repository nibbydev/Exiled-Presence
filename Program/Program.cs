using System.Linq;
using System.Text.RegularExpressions;

namespace Program {
    internal static class Program {
        /// <summary>
        /// Entry point
        /// </summary>
        public static void Main(string[] args) {
            // Hook service to menus
            MenuSystem.Menus.SessIdInputMenu.InputPropagateFunc = Service.Service.SessIdInputPropagate;
            MenuSystem.Menus.AccountNameInputMenu.InputPropagateFunc = Service.Service.AccountNameInputPropagate;
            MenuSystem.Menus.MainMenu.MenuItems.First().ActionToExecute = Service.Service.Init;
            
            // Run main menu
            MenuSystem.Menus.MainMenu.RunMenu();
        }
    }
}