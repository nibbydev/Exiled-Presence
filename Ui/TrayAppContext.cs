using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ui {
    internal class TrayAppContext : ApplicationContext {
        private const string WindowTitle = "Exiled Presence";
        private readonly NotifyIcon _trayItem;

        public TrayAppContext() {
            // Create a tray item
            _trayItem = new NotifyIcon {
                Text = WindowTitle,
                Icon = Properties.Resources.AppIcon,
                ContextMenu = new ContextMenu(new[] {
                    new MenuItem("Open menu", RunConsoleAsTask),
                    new MenuItem("Exit", Exit)
                }),
                Visible = true,
                BalloonTipIcon = ToolTipIcon.None,
                BalloonTipTitle = WindowTitle,
                BalloonTipText = $@"Right click the {WindowTitle} tray icon to access settings."
            };
            
            _trayItem.ShowBalloonTip(0);
            
            // Start the service
            Service.Service.Init();
        }

        private static void RunConsoleAsTask(object sender = null, EventArgs e = null) {
            if (ConsoleManager.IsConsoleVisible) {
                return;
            }

            new Task(() => {
                ConsoleManager.Show();
                MenuSystem.Menus.MainMenu.RunMenu();
                ConsoleManager.Hide();
            }).Start();
        }

        private void Exit(object sender = null, EventArgs e = null) {
            _trayItem.Visible = false;
            Service.Service.Stop();
            Application.Exit();
        }
        
        public void TooltipMsg(string ttMsg, string ttIcon = "none") {
            // Don't display empty messages
            if (string.IsNullOrEmpty(ttMsg)) {
                return;
            }

            // Attempt to map the string representation of ttIcon to its enum counterpart
            if (!Enum.TryParse(ttIcon, true, out ToolTipIcon ttIconEnum)) {
                return;
            }

            // Set values as display the tooltip
            _trayItem.BalloonTipIcon = ttIconEnum;
            _trayItem.BalloonTipText = ttMsg;
            _trayItem.ShowBalloonTip(0);
        }
    }
}