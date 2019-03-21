using System;
using System.Diagnostics;
using System.Globalization;
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
            _controller = new Controller(_settings);
            
            CreateContextMenuEntries();

            try {
                _config.Load();
                CheckUpdates();
                _controller.Initialize();
            } catch (Exception e) {
                // Config has invalid options
                TooltipMsg(e.Message, "error");
            }
        }

        /// <summary>
        /// Attempts to read config and reinitialize the service
        /// </summary>
        private void Reload(object sender = null, EventArgs args = null) {
            _controller.Dispose();
            
            try {
                _config.Load();
                _controller.Initialize();
                TooltipMsg("Reload successful");
            } catch (ArgumentException e) {
                // Config has invalid options
                TooltipMsg(e.Message, "error");
            }
        }

        /// <summary>
        /// Constructs the tray menu structure
        /// </summary>
        private void CreateContextMenuEntries() {
            _trayItem.ContextMenu.MenuItems.Add(
                new MenuItem("Edit config", delegate { _config.OpenInEditor(); })
            );

            _trayItem.ContextMenu.MenuItems.Add(new MenuItem("Reload service", Reload));

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
                new MenuItem("Open startup folder", delegate { Misc.OpenPath(Settings.StartupFolderPath); })
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
            
            Console.WriteLine(@"Checking updates...");

            var release = await Web.GetLatestRelease();
            if (release == null) {
                return;
            }

            _settings.UpdateLastUpdateTime();
            _config.Save();

            if (Misc.IsNewVersion(Settings.Version, release.tag_name)) {
                _releaseUrl = release.html_url;
                Console.WriteLine(@"New version is available");
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