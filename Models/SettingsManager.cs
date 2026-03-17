using System;
using System.IO;
using System.Text.Json;

namespace CalabiyauQuotation.Models
{
    public class AppSettings
    {
        public bool EnableAutoDownload { get; set; } = true;
        public string DictionaryUrl { get; set; } = "https://cdn.xcnahida.cn/api/meowQuotes/index.php";
        public string Hotkey { get; set; } = "Ctrl+Alt+P";
        public bool ClearAndPaste { get; set; } = true;
        public bool AutoSend { get; set; } = false;
        public string Language { get; set; } = "auto"; // auto, zh-CN, en

        public AppSettings Clone()
        {
            return new AppSettings
            {
                EnableAutoDownload = EnableAutoDownload,
                DictionaryUrl = DictionaryUrl,
                Hotkey = Hotkey,
                ClearAndPaste = ClearAndPaste,
                AutoSend = AutoSend,
                Language = Language
            };
        }
    }

    public static class SettingsManager
    {
        private static readonly string SettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppSettings.json");
        public static AppSettings Current { get; private set; } = new AppSettings();

        static SettingsManager()
        {
            Load();
        }

        public static void Load()
        {
            if (File.Exists(SettingsPath))
            {
                try
                {
                    string json = File.ReadAllText(SettingsPath);
                    Current = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
                catch
                {
                    Current = new AppSettings();
                }
            }
        }

        public static void Save()
        {
            string json = JsonSerializer.Serialize(Current, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsPath, json);
        }
    }
}
