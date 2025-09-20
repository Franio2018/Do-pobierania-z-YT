using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicDownloadLib
{
    internal interface IPlaylist
    {
        public Google.Apis.YouTube.v3.Data.Playlist apiPlaylist { get; }

        public List<PlaylistItem> GetVideos();
        public PlaylistItem GetVideo(int index);
        public bool RemoveVideo(int index);
        public bool RemoveRange(int index, int count);
        public bool RemoveVideoById(string id);
        public bool RemoveAll();

    }
}
