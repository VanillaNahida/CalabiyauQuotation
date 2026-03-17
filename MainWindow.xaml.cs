using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CalabiyauQuotation.Models;
using CalabiyauQuotation.Services;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Toolkit.Uwp.Notifications;
using Res = CalabiyauQuotation.Properties.Resources;

namespace CalabiyauQuotation
{
    public partial class MainWindow : Window
    {
        private HotKeyManager? _hotKeyManager;
        private bool _isExit = false;
        private AppSettings? _originalSettings;
        private bool _hasUnsavedChanges = false;
        private bool _isUpdatingSettings = false;
        private int _iconState = 0; // 0: AppIcon.ico, 1: XingHuiNotifation.png, 2: NotifationIcon.png

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSettings();
            CopyLocalDictionary();

            _hotKeyManager = new HotKeyManager(this);
            RegisterHotKeys();
            GenerateRandomSentence();

            if (SettingsManager.Current.EnableAutoDownload && !string.IsNullOrEmpty(SettingsManager.Current.DictionaryUrl))
            {
                DownloadDictionary();
            }

            UpdateStatus(string.Format(Res.StatusLoaded, DictionaryManager.Sentences.Count));
        }

        private void CopyLocalDictionary()
        {
            string sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Calabiyau_text.yml");
            if (!File.Exists(sourcePath))
            {
                string devPath = @"c:\Users\VanillaNahida\Documents\VS2022_Project\CalabiyauQuotation\src\Calabiyau_text.yml";
                if (File.Exists(devPath))
                {
                    File.Copy(devPath, sourcePath);
                    DictionaryManager.LoadLocalDictionary();
                }
            }
        }

        private void LoadSettings()
        {
            _isUpdatingSettings = true;
            _originalSettings = SettingsManager.Current.Clone();
            hkHotkey.HotKey = SettingsManager.Current.Hotkey;
            chkClearAndPaste.IsChecked = SettingsManager.Current.ClearAndPaste;
            chkAutoSend.IsChecked = SettingsManager.Current.AutoSend;
            chkEnableAutoDownload.IsChecked = SettingsManager.Current.EnableAutoDownload;
            txtDictionaryUrl.Text = SettingsManager.Current.DictionaryUrl;
            
            // 设置语言选择
            string language = SettingsManager.Current.Language;
            cmbLanguage.SelectedIndex = language switch
            {
                "zh-CN" => 1,
                "en" => 2,
                _ => 0 // auto
            };
            
            _isUpdatingSettings = false;
            UpdateUnsavedState();
        }

        private void RegisterHotKeys()
        {
            if (_hotKeyManager == null) return;

            _hotKeyManager.UnregisterAll();
            _hotKeyManager.RegisterHotKey(SettingsManager.Current.Hotkey, GenerateAndPaste);
        }

        private void GenerateAndPaste()
        {
            string sentence = DictionaryManager.GetRandomSentence();
            ClipboardService.CopyTextToClipboard(sentence);
            
            if (SettingsManager.Current.ClearAndPaste)
            {
                ClipboardService.SelectAllAndPaste();
            }
            else
            {
                ClipboardService.PasteText();
            }

            if (SettingsManager.Current.AutoSend)
            {
                ClipboardService.PressEnter();
            }
            
            Dispatcher.Invoke(() =>
            {
                txtSentence.Text = sentence;
                UpdateStatus(SettingsManager.Current.ClearAndPaste ? Res.StatusChangedAndPasted : Res.StatusChanged);
            });
        }

        private void GenerateRandomSentence()
        {
            txtSentence.Text = DictionaryManager.GetRandomSentence();
        }

        private void UpdateStatus(string message)
        {
            if (txtStatus != null)
            {
                txtStatus.Text = message;
            }
        }

        private bool CheckSettingsChanged()
        {
            if (_originalSettings == null || hkHotkey == null || chkClearAndPaste == null || 
                chkAutoSend == null || chkEnableAutoDownload == null || txtDictionaryUrl == null || cmbLanguage == null) 
                return false;

            string selectedLanguage = (cmbLanguage.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "auto";

            return hkHotkey.HotKey != _originalSettings.Hotkey ||
                   (chkClearAndPaste.IsChecked ?? true) != _originalSettings.ClearAndPaste ||
                   (chkAutoSend.IsChecked ?? false) != _originalSettings.AutoSend ||
                   (chkEnableAutoDownload.IsChecked ?? false) != _originalSettings.EnableAutoDownload ||
                   txtDictionaryUrl.Text != _originalSettings.DictionaryUrl ||
                   selectedLanguage != _originalSettings.Language;
        }

        private void UpdateUnsavedState()
        {
            if (_isUpdatingSettings || txtStatus == null) return;

            _hasUnsavedChanges = CheckSettingsChanged();

            if (_hasUnsavedChanges)
            {
                txtStatus.Text = Res.StatusUnsavedChanges;
                txtStatus.Foreground = System.Windows.Media.Brushes.Red;
            }
            else
            {
                txtStatus.Text = Res.StatusReadyAgain;
                txtStatus.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void Setting_Changed(object sender, RoutedEventArgs e)
        {
            UpdateUnsavedState();
        }

        private void HotKeyBox_HotKeyChanged(object? sender, EventArgs e)
        {
            UpdateUnsavedState();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabControl == null) return;
            
            if (tabControl.SelectedItem is TabItem selectedTab && selectedTab.Header.ToString() == Res.TabSettings)
            {
                SettingsManager.Load();
                LoadSettings();
            }
            else if (tabControl.SelectedItem is TabItem mainTab && mainTab.Header.ToString() == Res.TabMain)
            {
                if (txtStatus != null)
                {
                    txtStatus.Text = Res.StatusReadyAgain;
                    txtStatus.Foreground = System.Windows.Media.Brushes.Black;
                }
            }
        }

        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            GenerateRandomSentence();
            UpdateStatus(Res.StatusChanged);
        }

        private void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            ClipboardService.CopyTextToClipboard(txtSentence.Text);
            UpdateStatus(Res.StatusCopied);
        }

        private async void BtnDownload_Click(object sender, RoutedEventArgs e)
        {
            await DownloadDictionary();
        }

        private async System.Threading.Tasks.Task DownloadDictionary()
        {
            string url = txtDictionaryUrl.Text;
            if (string.IsNullOrEmpty(url))
            {
                UpdateStatus(Res.StatusNoDictionaryUrl);
                return;
            }

            UpdateStatus(Res.StatusDownloading);
            btnDownload.IsEnabled = false;

            var result = await DictionaryManager.DownloadDictionaryAsync(url);
            btnDownload.IsEnabled = true;

            if (result == DictionaryManager.DownloadResult.Success)
            {
                UpdateStatus(string.Format(Res.StatusDownloadSuccess, DictionaryManager.Sentences.Count));
                GenerateRandomSentence();
            }
            else if (result == DictionaryManager.DownloadResult.InvalidFormat)
            {
                UpdateStatus(Res.StatusDownloadFailedFormat);
            }
            else
            {
                UpdateStatus(Res.StatusDownloadFailed);
            }
        }

        private void BtnSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsManager.Current.Hotkey = hkHotkey.HotKey;
            SettingsManager.Current.ClearAndPaste = chkClearAndPaste.IsChecked ?? true;
            SettingsManager.Current.AutoSend = chkAutoSend.IsChecked ?? false;
            SettingsManager.Current.EnableAutoDownload = chkEnableAutoDownload.IsChecked ?? false;
            SettingsManager.Current.DictionaryUrl = txtDictionaryUrl.Text;
            SettingsManager.Current.Language = (cmbLanguage.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "auto";
            SettingsManager.Save();
            _originalSettings = SettingsManager.Current.Clone();
            _hasUnsavedChanges = false;
            RegisterHotKeys();
            UpdateUnsavedState();
            MessageBox.Show(Res.MsgSettingsSaved, Res.MsgSettingsSavedTitle, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // 发送Windows通知
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_isExit)
            {
                e.Cancel = true;
                Hide();
                ShowNotification(Res.AppName, Res.NotificationMinimized);
            }
            else
            {
                _hotKeyManager?.Dispose();
                taskbarIcon.Dispose();
            }
        }

        private void ShowNotification(string title, string message)
        {
            try
            {
                ToastNotificationHelper.ShowSimpleToast(title, message);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ShowNotification error: {ex.Message}");
                try
                {
                    taskbarIcon.ShowBalloonTip(title, message, Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
                }
                catch (Exception innerEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Fallback notification error: {innerEx.Message}");
                }
            }
        }

        private void MenuItem_Show_Click(object sender, RoutedEventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }

        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            _isExit = true;
            Application.Current.Shutdown();
        }

        private void TaskbarIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            if (IsVisible && WindowState == WindowState.Normal)
            {
                Hide();
            }
            else
            {
                Show();
                WindowState = WindowState.Normal;
                Activate();
            }
        }

        // 托盘菜单图标
        private void TaskbarIcon_TrayRightMouseDown(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            
            var contextMenu = new ContextMenu();
            
            var showMenuItem = new MenuItem { Header = Res.MenuItemShow };
            showMenuItem.Click += MenuItem_Show_Click;
            contextMenu.Items.Add(showMenuItem);
            
            contextMenu.Items.Add(new Separator());
            
            var exitMenuItem = new MenuItem { Header = Res.MenuItemExit };
            exitMenuItem.Click += MenuItem_Exit_Click;
            contextMenu.Items.Add(exitMenuItem);
            
            contextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
            contextMenu.IsOpen = true;
        }

        private void GitHubUrl_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://github.com/VanillaNahida/CalabiyauQuotation",
                    UseShellExecute = true
                });
            }
            catch
            {
            }
        }

        private void BilibiliUrl_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://space.bilibili.com/1347891621",
                    UseShellExecute = true
                });
            }
            catch
            {
            }
        }
        
        private void QQGroupUrl_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://www.bilibili.com/opus/1045130607332425735",
                    UseShellExecute = true
                });
            }
            catch
            {
            }
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                // 切换图标状态
                _iconState = (_iconState + 1) % 3;
                
                // 根据状态设置图标
                switch (_iconState)
                {
                    case 0:
                        aboutImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Assets/AppIcon.ico"));
                        break;
                    case 1:
                        aboutImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Assets/XingHuiNotifation.png"));
                        break;
                    case 2:
                        aboutImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/Assets/NotifationIcon.png"));
                        break;
                }
                
                // 发送通知
                string iconPath = ExtractXingHuiIconToTemp();
                ToastNotificationHelper.ShowToastWithLogo(Res.NotificationTitle, Res.NotificationMessage, iconPath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Image click error: {ex.Message}");
            }
        }

        private string ExtractXingHuiIconToTemp()
        {
            try
            {
                string tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "CalabiyauQuotation");
                if (!System.IO.Directory.Exists(tempDir))
                {
                    System.IO.Directory.CreateDirectory(tempDir);
                }

                string tempIconPath = System.IO.Path.Combine(tempDir, "XingHuiNotifation.png");

                if (System.IO.File.Exists(tempIconPath))
                {
                    return tempIconPath;
                }

                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                string resourceName = "CalabiyauQuotation.Assets.XingHuiNotifation.png";

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        using (var fileStream = System.IO.File.Create(tempIconPath))
                        {
                            stream.CopyTo(fileStream);
                        }
                        return tempIconPath;
                    }
                }

                var uri = new Uri("pack://application:,,,/Assets/XingHuiNotifation.png");
                var resourceStream = System.Windows.Application.GetResourceStream(uri);
                if (resourceStream != null)
                {
                    using (var fileStream = System.IO.File.Create(tempIconPath))
                    {
                        resourceStream.Stream.CopyTo(fileStream);
                    }
                    return tempIconPath;
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ExtractXingHuiIconToTemp error: {ex.Message}");
                return null;
            }
        }

        private void CmbLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingSettings) return;
            
            if (cmbLanguage.SelectedItem is ComboBoxItem item)
            {
                // 语言设置已更改，需要重启程序才能生效
                var result = MessageBox.Show(Res.MsgRestartRequired, Res.MsgRestartRequiredTitle, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    // 保存设置并重启程序
                    SettingsManager.Current.Language = (cmbLanguage.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "auto";
                    SettingsManager.Save();
                    
                    // 重启程序
                    string exePath = Environment.ProcessPath ?? System.Reflection.Assembly.GetEntryAssembly()?.Location ?? AppDomain.CurrentDomain.BaseDirectory;
                    if (exePath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                    {
                        exePath = System.IO.Path.ChangeExtension(exePath, ".exe");
                    }
                    
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = exePath,
                        UseShellExecute = true
                    });
                    
                    System.Windows.Application.Current.Shutdown();
                }
            }
        }
    }
}