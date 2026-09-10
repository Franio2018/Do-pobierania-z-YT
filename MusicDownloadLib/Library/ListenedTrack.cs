namespace MusicDownloadLib.Library;

public sealed record ListenedTrack(
    string VideoId,
    string Title,
    string? Url,
    int ListenCount,
    DateTimeOffset FirstListenedAt,
    DateTimeOffset LastListenedAt);
