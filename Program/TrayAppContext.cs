using System;
using System.Windows.Forms;
using Service;
using Utility;

namespace Program {
    public class TrayAppContext : ApplicationContext {
        private readonly Config _config = new Config();
        private readonly Controller _controller;
        private string _releaseUrl;

        private readonly NotifyIcon _trayItem = new NotifyIcon {
            Text = Settings.ProgramName,
            Icon = Properties.Resources.ico,
            BalloonTipTitle = Settings.ProgramName,
            ContextMenu = new ContextMenu(),
            Visible = true
        };

        /// <summary>
        /// Constructor
        /// </summary>
        public TrayAppContext() {
            CreateContextMenuEntries();

            if (!_config.LoadConfig(out var msg)) {
                TooltipMsg(msg, "error");
            }

            CheckUpdates();
            
            _controller  = new Controller(_config.Settings);
            _controller.Initialize();
        }

        /// <summary>
        /// Constructs the tray menu structure
        /// </summary>
        private void CreateContextMenuEntries() {
            _trayItem.ContextMenu.MenuItems.Add(
                new MenuItem("Edit config", delegate { _config.OpenConfig(); })
            );

            _trayItem.ContextMenu.MenuItems.Add(
                new MenuItem("Reload", delegate {
                    if (_config.LoadConfig(out var msg)) {
                        _controller.Dispose();
                        _controller.Initialize();

                        TooltipMsg("Reload successful");
                    } else TooltipMsg(msg, "error");
                })
            );

            _trayItem.ContextMenu.MenuItems.Add("Open..", new[] {
                new MenuItem("Config folder", delegate { General.OpenPath(Settings.CfgFolderPath); })
            });

            _trayItem.ContextMenu.MenuItems.Add(
                new MenuItem("Exit", delegate {
                    // Hide the icon so it doesn't persist
                    _trayItem.Visible = false;
                    _controller.Dispose();
                    Application.Exit();
                })
            );

            // Register event handlers
            _trayItem.BalloonTipClicked += OnBalloonClick;
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

        /// <summary>
        /// Checks for updates infrequently
        /// </summary>
        private async void CheckUpdates() {
            if (!_config.Settings.IsCheckUpdates()) {
                return;
            }

            var release = await Utility.Web.GetLatestRelease();
            if (release == null) {
                return;
            }

            _config.Settings.LastUpdateCheck = DateTime.UtcNow;
            _config.SaveConfig();

            if (Utility.General.IsNewVersion(Settings.Version, release.tag_name)) {
                _releaseUrl = release.html_url;
                TooltipMsg($"{release.tag_name} released. Click here to open in browser");
            }
        }

        /// <summary>
        /// Callback for when user clicks on popup message
        /// </summary>
        private void OnBalloonClick(object sender, EventArgs args) {
            if (!string.IsNullOrEmpty(_releaseUrl)) {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(_releaseUrl));
                _releaseUrl = null;
            }
        }
    }
}