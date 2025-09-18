using Google.Apis.YouTube.v3.Data;

namespace MusicDownloadLib
{
    internal interface IChannel
    {
        public string url { get; }
        public string name { get;}
        public string id { get; }
        public List<Playlist> playlists { get; set; }

        public bool ResolveChannelId();

        public bool GetChannelPlaylists();

        public Playlist GetPlaylist(int index);

        public bool ResolveChannelName();
    }
}
