using Google.Apis.YouTube.v3.Data;

namespace MusicDownloadLib.Playlists
{
    public static class TrackMapper
    {
        public static PlaylistItem ToPlaylistItem(string videoId, string title) =>
            new()
            {
                Snippet = new PlaylistItemSnippet
                {
                    Title = title,
                    ResourceId = new ResourceId { VideoId = videoId }
                }
            };

        public static PlaylistItem ToPlaylistItem(this ListenedTrack track) =>
            ToPlaylistItem(track.VideoId, track.Title);

        public static PlaylistItem ToPlaylistItem(this DownloadedTrack track) =>
            ToPlaylistItem(track.VideoId, track.Title);
    }
}
