namespace MusicDownloadLib.Library;

public sealed record DownloadedTrack(
    string VideoId,
    string Title,
    string? FilePath,
    string Status,
    string? Error,
    DateTimeOffset DownloadedAt);
