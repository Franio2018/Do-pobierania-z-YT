using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicDownloadLib
{
    public class Playlist : IPlaylist
    {
        private YouTubeService youtubeService = new YouTubeService(new BaseClientService.Initializer() { ApiKey = "AIzaSyADJG2lPWphTWLQDcyG9iBjoQmawmtc0uE" });

        public Google.Apis.YouTube.v3.Data.Playlist apiPlaylist { get; private set; }

        private List<PlaylistItem> videos = new();

        public Playlist(Google.Apis.YouTube.v3.Data.Playlist playlist)
        {
            apiPlaylist = playlist;
            FetchVideos();
        }

        // Test Purposes
        public Playlist(List<PlaylistItem> initialVideos)
        {
            apiPlaylist = new Google.Apis.YouTube.v3.Data.Playlist(); // dummy
            videos = new List<PlaylistItem>(initialVideos);
        }


        private bool FetchVideos()
        {
            var playlistRequest = youtubeService.PlaylistItems.List("snippet");
            playlistRequest.PlaylistId = apiPlaylist.Id;
            playlistRequest.MaxResults = 50;
            
            try
            {
                // Execute the playlist request
                var PlaylistResponse = playlistRequest.Execute();

                // Request playlist items
                var playlistItemsListRequest = youtubeService.PlaylistItems.List("snippet");
                playlistItemsListRequest.PlaylistId = apiPlaylist.Id;
                playlistItemsListRequest.MaxResults = 500; // Fetch up to 500 items in one request

                // Execute the request and get the response
                var playlistItemsListResponse = playlistItemsListRequest.Execute();

                videos.AddRange(playlistItemsListResponse.Items);
                return true;
            }
            catch (Exception ex)
            {
                // Log or display the error for the entire method
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            return false;
        }

        public bool RemoveAll()
        {
            videos.Clear();
            return true;
        }

        public bool RemoveRange(int index, int count)
        {
            if (index < 0 || count < 0 || index + count > videos.Count)
            {
                return false;
            }
            videos.RemoveRange(index, count);
            return true;
        }

        public bool RemoveVideo(int index)
        {
            if(index >=  videos.Count)
                return false; 
            videos.RemoveAt(index);
            return true;
        }

        public bool RemoveVideoById(string id)
        {
            int removed = videos.RemoveAll(v => v.Id == id);
            return removed > 0;
        }      

        public PlaylistItem GetVideo(int index)
        {
            if(index < 0 ||  index >= videos.Count)
                throw new IndexOutOfRangeException();
            return videos[index];
        }

        public List<PlaylistItem> GetVideos()
        {
            return [.. videos];
        }

        public string GetTitle()
        {
            return apiPlaylist.Snippet.Title;
        }

    }
}
