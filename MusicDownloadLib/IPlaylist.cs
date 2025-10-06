using Google.Apis.YouTube.v3.Data;

namespace MusicDownloadLib
{
    public interface IPlaylist
    {
        public Google.Apis.YouTube.v3.Data.Playlist apiPlaylist { get; }

        public List<PlaylistItem> GetVideos();
        public PlaylistItem GetVideo(int index);
        public bool RemoveVideo(int index);
        public bool RemoveRange(int index, int count);
        public bool RemoveVideoById(string id);
        public bool RemoveAll();
        public string GetTitle();

    }
}
