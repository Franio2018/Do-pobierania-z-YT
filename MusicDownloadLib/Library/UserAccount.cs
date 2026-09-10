namespace MusicDownloadLib.Library;

public sealed record UserAccount(
    string AccountId,
    string Name,
    string? Url,
    DateTimeOffset LastUsedAt);
