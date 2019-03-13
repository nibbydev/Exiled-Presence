using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Domain;
using Program.Properties;
using Service;

namespace Program {
    public class TrayAppContext : ApplicationContext {
        private readonly Settings _settings;
        private readonly Config _config;
        private readonly Controller _controller;
        private string _releaseUrl;

        private readonly NotifyIcon _trayItem = new NotifyIcon {
            Text = Settings.ProgramName,
            Icon = Resources.ico,
            BalloonTipTitle = Settings.ProgramName,
            ContextMenu = new ContextMenu(),
            Visible = true
        };

        /// <summary>
        /// Constructor
        /// </summary>
        public TrayAppContext() {
            _settings = new Settings();
            _config = new Config(_settings);
            
            CreateContextMenuEntries();

            if (!_config.LoadConfig(out var msg)) {
                TooltipMsg(msg, "error");
            }

            CheckUpdates();
            
            _controller  = new Controller(_settings);
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
                new MenuItem("Reload service", delegate {
                    if (_config.LoadConfig(out var msg)) {
                        _controller.Dispose();
                        _controller.Initialize();

                        TooltipMsg("Reload successful");
                    } else TooltipMsg(msg, "error");
                })
            );

            _trayItem.ContextMenu.MenuItems.Add("Startup..", new[] {
                new MenuItem("Add to startup", delegate {
                    TooltipMsg(File.Exists(Settings.StartupShortcutPath)
                        ? "Recreated startup shortcut"
                        : "Created startup shortcut", "info");
                    Win32.CreateShortcut(Settings.AppPath, Settings.StartupShortcutPath);
                }),
                new MenuItem("Remove from startup", delegate {
                    if (File.Exists(Settings.StartupShortcutPath)) {
                        File.Delete(Settings.StartupShortcutPath);
                        TooltipMsg("Deleted startup shortcut", "info");
                    } else TooltipMsg("Startup shortcut did not exist", "error");
                }),
                new MenuItem("Open startup folder", delegate {
                    Misc.OpenPath(Settings.StartupFolderPath);
                })
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
            if (!_settings.IsCheckUpdates()) {
                return;
            }

            var release = await Web.GetLatestRelease();
            if (release == null) {
                return;
            }

            _settings.LastUpdateCheck = DateTime.UtcNow;
            _config.SaveConfig();

            if (Misc.IsNewVersion(Settings.Version, release.tag_name)) {
                _releaseUrl = release.html_url;
                TooltipMsg($"{release.tag_name} released. Click here to open in browser");
            }
        }

        /// <summary>
        /// Callback for when user clicks on popup message
        /// </summary>
        private void OnBalloonClick(object sender, EventArgs args) {
            if (!string.IsNullOrEmpty(_releaseUrl)) {
                Process.Start(new ProcessStartInfo(_releaseUrl));
                _releaseUrl = null;
            }
        }
    }
}