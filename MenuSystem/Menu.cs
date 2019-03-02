using System;
using System.Linq;

namespace MenuSystem {
    public class Menu {
        public string Title { get; set; }
        public MenuItem[] MenuItems { get; set; }
        public bool ClearConsole { get; set; } = true;
        public bool IsInput { get; set; }
        public bool DisableGoBack { get; set; }
        public Action PrintActionToExecute { get; set; }
        public Func<string, string> InputPropagateFunc { get; set; }
        public string[] Description { get; set; }

        private void PrintMenu() {
            if (ClearConsole) {
                Console.Clear();
            }

            PrintHr(Title);

            if (Description != null) {
                foreach (var s in Description) Console.WriteLine($" {s}");
                PrintHr();
            }

            if (PrintActionToExecute != null) {
                PrintActionToExecute();
                PrintHr();
            }

            // Does the set of menu items contain a default choice?
            var hasDefaultChoice = false;

            if (MenuItems != null) {
                foreach (var menuItem in MenuItems) {
                    if (menuItem.IsDefaultChoice) {
                        hasDefaultChoice = true;
                        
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine(menuItem);
                        Console.ResetColor();
                        continue;
                    }

                    Console.WriteLine(menuItem);
                }

                PrintHr();
            }

            if (!DisableGoBack) {
                // There was no default choice, go back item will be default
                if (!hasDefaultChoice) {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                }

                Console.WriteLine(Menus.MainMenu.Equals(this) ? Menus.ExitProgramItem : Menus.GoBackItem);
                Console.ResetColor();
            }

            Console.Write("> ");
        }

        private static void PrintHr(string title = null) {
            const int width = 64;

            if (title == null) {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("[>" + new string('=', width - 4) + "<]");
                Console.ResetColor();
            } else {
                if (title.Length % 2 != 0) title += " ";
                var hr = "[>" + new string('=', (width - title.Length - 2) / 2 - 4) + "<]";

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"{hr} ");
                Console.ResetColor();

                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write(title);
                Console.ResetColor();

                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($" {hr}");
                Console.ResetColor();
            }
        }

        private void PropagateInput(string input) {
            // Pass the user input to the propagate function which should return a display string indicating
            // whether the action was successful or not
            var display = InputPropagateFunc?.Invoke(input);

            // No display string was returned
            if (string.IsNullOrEmpty(display)) {
                display = "ERROR. This command does not have an action defined!";
            }
                    
            // Display a feedback menu with that display
            new Menu {
                Title = "Input",
                Description = new[] {display}
            }.RunMenu();
        }

        public string RunMenu() {
            while (true) {
                PrintMenu();
                var input = Console.ReadLine()?.Trim();

                // User just pressed enter and no other input
                if (string.IsNullOrEmpty(input)) {
                    // If there are no menus or no default choices
                    if (MenuItems == null || !MenuItems.Any(t => t.IsDefaultChoice)) {
                        return null;
                    }
                }

                // User's input is GoBackItem's shortcut
                if (Menus.GoBackItem.Shortcut.Equals(input?.ToUpper())) {
                    return null;
                }
                
                // No default choice, go back
                if (MenuItems == null && !IsInput) {
                    return null;
                }

                // Menu is input menu
                if (IsInput) {
                    PropagateInput(input);
                    return null;
                }
                
                // No default choice, go back
                if (MenuItems == null) {
                    return null;
                }

                // Load user-specified or default menu item
                var item = string.IsNullOrEmpty(input)
                    ? MenuItems.FirstOrDefault(m => m.IsDefaultChoice)
                    : MenuItems.FirstOrDefault(m => m.Shortcut == input);

                // The menu item was null
                if (item == null) {
                    Console.WriteLine("Unknown input!");
                    Console.ReadKey(true);
                    continue;
                }

                // execute the command specified in the menu item
                if (item.MenuToRun == null && item.ActionToExecute == null) {
                    Console.WriteLine($"'{item.Description}' has no command assigned to it...");
                    Console.ReadKey(true);
                    continue;
                }

                if (item.ClearConsole) {
                    Console.Clear();
                }

                // If the selected menu item had an action, execute it
                item.ActionToExecute?.Invoke();
                // If the selected menu item had a menu, run it
                input = item.MenuToRun?.RunMenu();

                if (input == null) {
                    continue;
                }

                return input;
            }
        }
    }
}