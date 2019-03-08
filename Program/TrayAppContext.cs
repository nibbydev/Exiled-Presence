using System;
using System.Windows.Forms;
using Service;

namespace Program {
    public class TrayAppContext : ApplicationContext {
        private string _releaseUrl;

        private readonly NotifyIcon _trayItem = new NotifyIcon {
            Text = Settings.ProgramName,
            Icon = Properties.Resources.ico,
            BalloonTipTitle = Settings.ProgramName,
            ContextMenu = new ContextMenu(),
            Visible = true
        };


        public TrayAppContext() {
            _trayItem.ContextMenu.MenuItems.Add(new MenuItem("Restart service", RestartService));
            _trayItem.ContextMenu.MenuItems.Add(new MenuItem("Edit config", delegate { Config.OpenConfig(); }));
            _trayItem.ContextMenu.MenuItems.Add(new MenuItem("Reload config", ReloadConfig));
            _trayItem.ContextMenu.MenuItems.Add(new MenuItem("Exit", Exit));

            if (!Config.LoadConfig()) {
                TooltipMsg("Right click the tray icon to access settings");
            }

            CheckUpdates();
            Service.Service.Init();
        }
        
        private static void ReloadConfig(object sender = null, EventArgs args = null) {
            Config.LoadConfig();
            RestartService();
        }

        private static void RestartService(object sender = null, EventArgs args = null) {
            Service.Service.Stop();
            Service.Service.Init();
        }

        /// <summary>
        /// Exists the tray app safely
        /// </summary>
        private void Exit(object sender = null, EventArgs e = null) {
            // Hide the icon so it doesn't persist
            _trayItem.Visible = false;
            Service.Service.Stop();
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