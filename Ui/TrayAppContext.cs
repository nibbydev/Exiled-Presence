using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Service;

namespace Ui {
    internal class TrayAppContext : ApplicationContext {
        private readonly NotifyIcon _trayItem;
        private string _releaseUrl;

        public TrayAppContext() {
            // Create a tray item
            _trayItem = new NotifyIcon {
                Text = Settings.ProgramName,
                Icon = Properties.Resources.AppIcon,
                BalloonTipTitle = Settings.ProgramName,
                ContextMenu = new ContextMenu(new[] {
                    new MenuItem("Open menu", RunConsoleAsTask),
                    new MenuItem("Exit", Exit)
                }),
                Visible = true
            };

            // Display a tooltip
            TooltipMsg($@"Right click the {Settings.ProgramName} tray icon to access settings.");
            CheckUpdates();
            
            // Start the service
            Service.Service.Init();
        }

        /// <summary>
        /// Shows the console and runs the menu system as a task
        /// </summary>
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

        /// <summary>
        /// Exists the tray app safely
        /// </summary>
        private void Exit(object sender = null, EventArgs e = null) {
            _trayItem.Visible = false;
            Service.Service.Stop();
            ConsoleManager.Deallocate();
            Application.Exit();
        }

        /// <summary>
        /// Displays a popup message to the user
        /// </summary>
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
        
        private async void CheckUpdates() {
            if (!Config.Settings.IsCheckUpdates()) {
                return;
            }
            
            var release = await Utility.Web.GetLatestRelease();
            if (release == null) {
                return;
            }
            
            Config.Settings.LastUpdateCheck = DateTime.UtcNow;
            Config.SaveConfig();

            if (Utility.General.IsNewVersion(Settings.Version, release.tag_name)) {
                _trayItem.BalloonTipClicked += OnBalloonClick;
                _releaseUrl = release.html_url;
                TooltipMsg($"{release.tag_name} released. Click here to download.");
            }
        }
        
        private void OnBalloonClick(object sender, EventArgs args) {
            _trayItem.BalloonTipClicked -= OnBalloonClick;
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(_releaseUrl));
        }
    }
}