using System;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace CalabiyauQuotation.Services
{
    public static class ToastNotificationHelper
    {
        private const string AppId = "CalabiyauQuotation.卡拉彼丘喵语生成器喵";
        private static bool _isInitialized = false;
        private static string _appIconPath;

        public static void Initialize()
        {
            if (_isInitialized) return;

            try
            {
                _appIconPath = ExtractIconToTemp();

                ToastNotificationManagerCompat.OnActivated += OnToastActivated;
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ToastNotificationHelper.Initialize error: {ex.Message}");
            }
        }

        private static string ExtractIconToTemp()
        {
            try
            {
                string tempDir = Path.Combine(Path.GetTempPath(), "CalabiyauQuotation");
                if (!Directory.Exists(tempDir))
                {
                    Directory.CreateDirectory(tempDir);
                }

                string tempIconPath = Path.Combine(tempDir, "NotifationIcon.png");

                if (File.Exists(tempIconPath))
                {
                    return tempIconPath;
                }

                var assembly = Assembly.GetExecutingAssembly();
                string resourceName = "CalabiyauQuotation.Assets.NotifationIcon.png";

                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        using (var fileStream = File.Create(tempIconPath))
                        {
                            stream.CopyTo(fileStream);
                        }
                        return tempIconPath;
                    }
                }

                var uri = new Uri("pack://application:,,,/Assets/NotifationIcon.png");
                var resourceStream = System.Windows.Application.GetResourceStream(uri);
                if (resourceStream != null)
                {
                    using (var fileStream = File.Create(tempIconPath))
                    {
                        resourceStream.Stream.CopyTo(fileStream);
                    }
                    return tempIconPath;
                }

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ExtractIconToTemp error: {ex.Message}");
                return null;
            }
        }

        private static void OnToastActivated(ToastNotificationActivatedEventArgsCompat e)
        {
            try
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    if (System.Windows.Application.Current.MainWindow != null)
                    {
                        System.Windows.Application.Current.MainWindow.Show();
                        System.Windows.Application.Current.MainWindow.WindowState = System.Windows.WindowState.Normal;
                        System.Windows.Application.Current.MainWindow.Activate();
                    }
                });
            }
            catch
            {
            }
        }

        public static void ShowSimpleToast(string title, string message)
        {
            try
            {
                if (!_isInitialized)
                {
                    Initialize();
                }

                var binding = new ToastBindingGeneric()
                {
                    Children =
                    {
                        new AdaptiveText() { Text = title },
                        new AdaptiveText() { Text = message }
                    }
                };

                if (!string.IsNullOrEmpty(_appIconPath) && File.Exists(_appIconPath))
                {
                    binding.AppLogoOverride = new ToastGenericAppLogo()
                    {
                        Source = "file:///" + _appIconPath.Replace("\\", "/"),
                        HintCrop = ToastGenericAppLogoCrop.Default
                    };
                }

                var content = new ToastContent()
                {
                    Visual = new ToastVisual()
                    {
                        BindingGeneric = binding
                    },
                    Duration = ToastDuration.Short
                };

                var doc = new XmlDocument();
                doc.LoadXml(content.GetContent());

                var toast = new ToastNotification(doc);

                toast.Failed += (sender, args) =>
                {
                    System.Diagnostics.Debug.WriteLine($"Toast notification failed: {args.ErrorCode}");
                };

                ToastNotificationManagerCompat.CreateToastNotifier().Show(toast);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ShowSimpleToast error: {ex.Message}");
                throw;
            }
        }

        public static void ShowToastWithLogo(string title, string message, string logoPath = null)
        {
            try
            {
                if (!_isInitialized)
                {
                    Initialize();
                }

                var binding = new ToastBindingGeneric()
                {
                    Children =
                    {
                        new AdaptiveText() { Text = title },
                        new AdaptiveText() { Text = message }
                    }
                };

                string iconToUse = !string.IsNullOrEmpty(logoPath) && File.Exists(logoPath) 
                    ? logoPath 
                    : _appIconPath;

                if (!string.IsNullOrEmpty(iconToUse) && File.Exists(iconToUse))
                {
                    binding.AppLogoOverride = new ToastGenericAppLogo()
                    {
                        Source = "file:///" + iconToUse.Replace("\\", "/"),
                        HintCrop = ToastGenericAppLogoCrop.Default
                    };
                }

                var content = new ToastContent()
                {
                    Visual = new ToastVisual()
                    {
                        BindingGeneric = binding
                    },
                    Duration = ToastDuration.Short
                };

                var doc = new XmlDocument();
                doc.LoadXml(content.GetContent());

                var toast = new ToastNotification(doc);

                ToastNotificationManagerCompat.CreateToastNotifier().Show(toast);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ShowToastWithLogo error: {ex.Message}");
                throw;
            }
        }
    }
}
