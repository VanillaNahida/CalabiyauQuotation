using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CalabiyauQuotation.Models
{
    public class SentenceData
    {
        public List<string> Sentences { get; set; } = new List<string>();
    }

    public static class DictionaryManager
    {
        private static readonly string DictionaryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Calabiyau_text.yml");
        private static readonly Random Random = new Random();
        public static List<string> Sentences { get; private set; } = new List<string>();

        static DictionaryManager()
        {
            LoadLocalDictionary();
        }

        public static void LoadLocalDictionary()
        {
            if (File.Exists(DictionaryPath))
            {
                try
                {
                    string yaml = File.ReadAllText(DictionaryPath);
                    var deserializer = new DeserializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build();
                    var data = deserializer.Deserialize<SentenceData>(yaml);
                    Sentences = data.Sentences ?? new List<string>();
                }
                catch
                {
                    Sentences = new List<string>();
                }
            }
        }

        public static async Task<DownloadResult> DownloadDictionaryAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
                return DownloadResult.InvalidUrl;

            try
            {
                using var client = new HttpClient();
                string content = await client.GetStringAsync(url);
                
                if (!ValidateDictionaryFormat(content))
                    return DownloadResult.InvalidFormat;
                
                await File.WriteAllTextAsync(DictionaryPath, content);
                LoadLocalDictionary();
                return DownloadResult.Success;
            }
            catch
            {
                return DownloadResult.NetworkError;
            }
        }

        public enum DownloadResult
        {
            Success,
            InvalidUrl,
            NetworkError,
            InvalidFormat
        }

        private static bool ValidateDictionaryFormat(string yaml)
        {
            try
            {
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();
                var data = deserializer.Deserialize<SentenceData>(yaml);
                
                if (data == null || data.Sentences == null)
                    return false;
                
                return data.Sentences.Count > 0;
            }
            catch
            {
                return false;
            }
        }

        public static string GetRandomSentence()
        {
            if (Sentences.Count == 0)
                return "没有可用的喵语文本喵";
            
            return Sentences[Random.Next(Sentences.Count)];
        }
    }
}
