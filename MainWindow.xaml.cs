using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CalabiyauQuotation.Models;
using CalabiyauQuotation.Services;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Toolkit.Uwp.Notifications;

namespace CalabiyauQuotation
{
    public partial class MainWindow : Window
    {
        private HotKeyManager? _hotKeyManager;
        private bool _isExit = false;
        private AppSettings? _originalSettings;
        private bool _hasUnsavedChanges = false;
        private bool _isUpdatingSettings = false;

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

            UpdateStatus($"已加载 {DictionaryManager.Sentences.Count} 条喵语文本");
        }

        private void CopyLocalDictionary()
        {
            string sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CalabiYau_text.yml");
            if (!File.Exists(sourcePath))
            {
                string devPath = @"c:\Users\VanillaNahida\Documents\VS2022_Project\CalabiyauQuotation\src\CalabiYau_text.yml";
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
                UpdateStatus(SettingsManager.Current.ClearAndPaste ? "已更换并粘贴喵语文本" : "已更换喵语文本~");
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
                chkAutoSend == null || chkEnableAutoDownload == null || txtDictionaryUrl == null) 
                return false;

            return hkHotkey.HotKey != _originalSettings.Hotkey ||
                   (chkClearAndPaste.IsChecked ?? true) != _originalSettings.ClearAndPaste ||
                   (chkAutoSend.IsChecked ?? false) != _originalSettings.AutoSend ||
                   (chkEnableAutoDownload.IsChecked ?? false) != _originalSettings.EnableAutoDownload ||
                   txtDictionaryUrl.Text != _originalSettings.DictionaryUrl;
        }

        private void UpdateUnsavedState()
        {
            if (_isUpdatingSettings || txtStatus == null) return;

            _hasUnsavedChanges = CheckSettingsChanged();

            if (_hasUnsavedChanges)
            {
                txtStatus.Text = "有尚未保存的更改喵！";
                txtStatus.Foreground = System.Windows.Media.Brushes.Red;
            }
            else
            {
                txtStatus.Text = "已准备就绪喵";
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
            
            if (tabControl.SelectedItem is TabItem selectedTab && selectedTab.Header.ToString() == "设置")
            {
                SettingsManager.Load();
                LoadSettings();
            }
            else if (tabControl.SelectedItem is TabItem mainTab && mainTab.Header.ToString() == "主界面")
            {
                if (txtStatus != null)
                {
                    txtStatus.Text = "已准备就绪喵";
                    txtStatus.Foreground = System.Windows.Media.Brushes.Black;
                }
            }
        }

        private void BtnGenerate_Click(object sender, RoutedEventArgs e)
        {
            GenerateRandomSentence();
            UpdateStatus("已更换喵语文本喵");
        }

        private void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            ClipboardService.CopyTextToClipboard(txtSentence.Text);
            UpdateStatus("已复制到剪贴板喵");
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
                UpdateStatus("请先设置词库地址喵！");
                return;
            }

            UpdateStatus("正在下载词库...");
            btnDownload.IsEnabled = false;

            var result = await DictionaryManager.DownloadDictionaryAsync(url);
            btnDownload.IsEnabled = true;

            if (result == DictionaryManager.DownloadResult.Success)
            {
                UpdateStatus($"词库下载成功，共 {DictionaryManager.Sentences.Count} 条");
                GenerateRandomSentence();
            }
            else if (result == DictionaryManager.DownloadResult.InvalidFormat)
            {
                UpdateStatus("词库下载失败：词库格式不合法！");
            }
            else
            {
                UpdateStatus("词库下载失败！");
            }
        }

        private void BtnSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsManager.Current.Hotkey = hkHotkey.HotKey;
            SettingsManager.Current.ClearAndPaste = chkClearAndPaste.IsChecked ?? true;
            SettingsManager.Current.AutoSend = chkAutoSend.IsChecked ?? false;
            SettingsManager.Current.EnableAutoDownload = chkEnableAutoDownload.IsChecked ?? false;
            SettingsManager.Current.DictionaryUrl = txtDictionaryUrl.Text;
            SettingsManager.Save();
            _originalSettings = SettingsManager.Current.Clone();
            _hasUnsavedChanges = false;
            RegisterHotKeys();
            UpdateUnsavedState();
            MessageBox.Show("设置已保存喵", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // 发送Windows通知
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_isExit)
            {
                e.Cancel = true;
                Hide();
                ShowNotification("喵语生成器", "已最小化到系统托盘喵~");
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
            
            var showMenuItem = new MenuItem { Header = "显示主界面" };
            showMenuItem.Click += MenuItem_Show_Click;
            contextMenu.Items.Add(showMenuItem);
            
            contextMenu.Items.Add(new Separator());
            
            var exitMenuItem = new MenuItem { Header = "退出" };
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
    }
}
