using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ui {
    internal class TrayAppContext : ApplicationContext {
        private readonly NotifyIcon _notifyIcon;

        public TrayAppContext() {
            // Create a taskbar icon
            _notifyIcon = new NotifyIcon {
                Text = @"Exiled Presence",
                Icon = Properties.Resources.AppIcon,
                ContextMenu = new ContextMenu(new[] {
                    new MenuItem("Show console", ShowConsole), 
                    new MenuItem("Exit", Exit)
                }),
                Visible = true
            };
            
            // Allocate and display a console
            ConsoleManager.Allocate();
            // Run the main menu
            MenuSystem.Menus.MainMenu.RunMenu();
            // Hide the console after user has exited menu
            ConsoleManager.Hide();
        }

        private void ShowConsole(object sender, EventArgs e) {
            ConsoleManager.Show();
            MenuSystem.Menus.MainMenu.RunMenu();
            ConsoleManager.Hide();
        }

        private void Exit(object sender, EventArgs e) {
            _notifyIcon.Visible = false;
            Service.Service.Stop();
            Application.Exit();
        }
    }
}