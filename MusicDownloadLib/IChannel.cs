using Google.Apis.YouTube.v3.Data;

namespace MusicDownloadLib
{
    public interface IChannel
    {
        public string url { get; }
        public string name { get;}
        public string id { get; }

        public List<Playlist> playlists { get; }

        public Playlist GetPlaylist(int index);
    }
}
