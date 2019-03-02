using System.Linq;
using System.Text.RegularExpressions;

namespace Program {
    internal static class Program {
        public static void Main(string[] args) {
            MenuSystem.Menus.SessIdInputMenu.InputPropagateFunc = SessIdInputPropagate;
            MenuSystem.Menus.AccountNameInputMenu.InputPropagateFunc = AccountNameInputPropagate;
            MenuSystem.Menus.MainMenu.MenuItems.First().ActionToExecute = Service.Service.Init;
            
            MenuSystem.Menus.MainMenu.RunMenu();
        }
        
        private static string AccountNameInputPropagate(string input) {
            if (input.Length < 3) {
                return "ERROR. Invalid account name passed!";
            }

            Service.Service.AccountName = input;
            return "OK. Account name set.";
        }

        private static string SessIdInputPropagate(string input) {
            var regex = new Regex(@"^[0-9a-fA-F]{32}$");
            if (!regex.Match(input).Success) {
                return "ERROR. Invalid POESESSID passed!";
            }
            
            Service.Service.SessId = input;
            return "OK. POESESSID set.";
        }
    }
}