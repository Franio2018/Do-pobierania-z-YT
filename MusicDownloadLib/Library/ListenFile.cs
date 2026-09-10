using System.Text.Json;
using System.Text.Json.Serialization;

namespace MusicDownloadLib.Library
{
    public sealed class ListenExportFile
    {
        [JsonPropertyName("version")]
        public int Version { get; set; } = 1;

        [JsonPropertyName("exportedAt")]
        public string? ExportedAt { get; set; }

        [JsonPropertyName("listens")]
        public List<ListenExportItem> Listens { get; set; } = new();
    }

    public sealed class ListenExportItem
    {
        [JsonPropertyName("videoId")]
        public string VideoId { get; set; } = "";

        [JsonPropertyName("title")]
        public string Title { get; set; } = "";

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("listenCount")]
        public int ListenCount { get; set; } = 1;

        [JsonPropertyName("listenedAt")]
        public string? ListenedAt { get; set; }
    }

    public static class ListenFile
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        public static string PendingPath =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MusicDownload",
                "pending-listens.json");

        public static string DownloadsPath =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Downloads",
                "pending-listens.json");

        public static IReadOnlyList<string> FindPendingFiles()
        {
            var paths = new List<string>();
            AddExisting(paths, PendingPath);

            var downloadsDir = Path.GetDirectoryName(DownloadsPath);
            if (downloadsDir is not null && Directory.Exists(downloadsDir))
            {
                foreach (var path in Directory.EnumerateFiles(downloadsDir, "pending-listens*.json"))
                    AddExisting(paths, path);
            }

            return paths;
        }

        public static bool TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static ListenExportFile Parse(string json)
        {
            var file = JsonSerializer.Deserialize<ListenExportFile>(json, JsonOptions)
                ?? throw new InvalidOperationException("Listen file is empty.");
            return file;
        }

        public static ListenExportFile Load(string path) =>
            Parse(File.ReadAllText(path));

        public static void Save(string path, ListenExportFile file)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            file.ExportedAt = DateTimeOffset.UtcNow.ToString("O");
            File.WriteAllText(path, JsonSerializer.Serialize(file, JsonOptions));
        }

        public static void AppendPending(ListenExportItem item)
        {
            var file = File.Exists(PendingPath)
                ? Load(PendingPath)
                : new ListenExportFile();

            var existing = file.Listens.FirstOrDefault(l => l.VideoId == item.VideoId);
            if (existing is null)
                file.Listens.Add(item);
            else
            {
                existing.ListenCount = Math.Max(existing.ListenCount, 1) + 1;
                existing.Title = string.IsNullOrWhiteSpace(item.Title) ? existing.Title : item.Title;
                existing.Url = item.Url ?? existing.Url;
                existing.ListenedAt = item.ListenedAt ?? existing.ListenedAt;
            }

            Save(PendingPath, file);
        }

        private static void AddExisting(List<string> paths, string path)
        {
            if (!File.Exists(path))
                return;
            if (paths.Exists(existing => string.Equals(existing, path, StringComparison.OrdinalIgnoreCase)))
                return;
            paths.Add(path);
        }
    }
}
