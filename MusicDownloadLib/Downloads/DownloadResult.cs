namespace MusicDownloadLib.Downloads;

public sealed record DownloadResult(
    string VideoId,
    string Title,
    string FilePath,
    bool WroteNewFile);
