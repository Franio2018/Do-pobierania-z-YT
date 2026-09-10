using Google.Apis.YouTube.v3.Data;
using Playlist = MusicDownloadLib.Playlists.Playlist;

namespace MusicDownloadLib.Channels
{
    public interface IChannel
    {
        public string url { get; }
        public string name { get;}
        public string id { get; }

        public List<Playlist> playlists { get; }
        public List<PlaylistItem> videos { get; }

        public Playlist GetPlaylist(int index);
    }
}
