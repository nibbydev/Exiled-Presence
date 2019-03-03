using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ui {
    internal class TrayAppContext : ApplicationContext {
        private const string WindowTitle = "Exiled Presence";
        private readonly NotifyIcon _trayItem;
        private bool _isBalloonExpired;

        public TrayAppContext() {
            // Create a tray item
            _trayItem = new NotifyIcon {
                Text = WindowTitle,
                Icon = Properties.Resources.AppIcon,
                ContextMenu = new ContextMenu(new[] {
                    new MenuItem("Show console", RunConsoleAsTask), 
                    new MenuItem("Exit", Exit)
                }),
                Visible = true,
                BalloonTipIcon = ToolTipIcon.None,
                BalloonTipTitle = WindowTitle,
                BalloonTipText = $@"{WindowTitle} will continue to run in the tray. Right click to open console again."
            };
            
            // Run as task to not hold up the main process
            RunConsoleAsTask();
        }

        private void RunConsoleAsTask(object sender = null, EventArgs e = null) {
            if (ConsoleManager.IsConsoleVisible) {
                return;
            }
            
            new Task(() => {
                RunConsoleMenu();
                
                // Show info balloon once
                if (_isBalloonExpired) return;
                _trayItem.ShowBalloonTip(0);
                _isBalloonExpired = true;
            }).Start();
        }

        private static void RunConsoleMenu() {
            ConsoleManager.Show();
            MenuSystem.Menus.MainMenu.RunMenu();
            ConsoleManager.Hide();
        }

        private void Exit(object sender = null, EventArgs e = null) {
            _trayItem.Visible = false;
            Service.Service.Stop();
            Application.Exit();
        }
    }
}