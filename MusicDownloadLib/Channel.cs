using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System.Linq;

namespace MusicDownloadLib
{
    public class Channel : IChannel, IComparable<Channel>
    {
        private YouTubeService youtubeService = new YouTubeService(new BaseClientService.Initializer() { ApiKey = "AIzaSyADJG2lPWphTWLQDcyG9iBjoQmawmtc0uE" });
        public string ?url { get; private set; }
        public string ?name { get; private set; }
        public string ?id { get; private set; }
        public List<Playlist> playlists {  get; set; }

        public Channel(string url, string name, string id) {
            if (!string.IsNullOrEmpty(url))
            {
                this.url = url;
                if (!ResolveChannelId()) throw new Exception("Couldn't resolve channel ID based on URL");
                if(!ResolveChannelName()) throw new Exception("Couldn't resolve channel name based on URL");
            }
            else if (!string.IsNullOrEmpty(name))
            {
                this.name = name;
                if (!ResolveChannelId()) throw new Exception("Couldn't resolve channel ID based on name");
            }
            else if (!string.IsNullOrEmpty(id))
            {
                this.id = id;
                if (!ResolveChannelName()) throw new Exception("Couldn't resolve channel name based on ID");
            }
            else
            {
                throw new ArgumentNullException("URL/Name/Id value is requaiered!");
            }
            playlists = new List<Playlist>();
            GetChannelPlaylists();
        }

        public bool GetChannelPlaylists()
        {
            // Request playlists for the channel
            var playlistRequest = youtubeService.Playlists.List("snippet");

            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentNullException(nameof(id), "id cannot be null or empty.");
            }

            playlistRequest.ChannelId = id;
            playlistRequest.MaxResults = 50; // Fetch up to 50 playlists at a time

            var nextPageToken = "";
            do
            {
                playlistRequest.PageToken = nextPageToken;

                // Execute the request
                var playlistResponse = playlistRequest.Execute();

                // Add playlists from searched channel to playlist List
                playlists.AddRange(playlistResponse.Items);

                // Update the nextPageToken to fetch more pages
                nextPageToken = playlistResponse.NextPageToken;

            } while (!string.IsNullOrEmpty(nextPageToken));
            return true;
        }

        public Playlist GetPlaylist(int index)
        {
            if(index >= 0 && index < playlists.Count)
                return playlists[index];
            throw new IndexOutOfRangeException();
        }

        public bool ResolveChannelId()
        {
            if (!string.IsNullOrEmpty(url) && url.Contains("youtube.com"))
            {
                if (url.Contains("/channel/"))
                {
                    // Extract the channel ID directly
                    var parts = url.Split('/');
                    id = parts[^1]; // Get the last part of the URL
                    return true;
                }
                else
                {
                    // Use the Search API for custom/legacy URLs
                    var searchRequest = youtubeService.Search.List("snippet");
                    searchRequest.Q = url;
                    searchRequest.Type = "channel";
                    searchRequest.MaxResults = 1;

                    var searchResponse = searchRequest.Execute();
                    foreach (var result in searchResponse.Items)
                    {
                        id = result.Snippet.ChannelId;
                        return true;
                    }
                }
            }
            else
            {
                // Treat the input as a plain channel name
                var searchRequest = youtubeService.Search.List("snippet");
                searchRequest.Q = name;
                searchRequest.Type = "channel";
                searchRequest.MaxResults = 10;

                var searchResponse = searchRequest.Execute();
                id = searchResponse.Items[0].Snippet.ChannelId;
                return true;
            }
            return false;
        }

        public bool ResolveChannelName()
        {
            var searchRequest = youtubeService.Search.List("snippet");
            searchRequest.Q = id;
            searchRequest.Type = "channel";
            searchRequest.MaxResults = 10;

            var searchResponse = searchRequest.Execute();
            name = searchResponse.Items[0].Snippet.ChannelTitle;
            return true;
        }

        public int CompareTo(Channel? other)
        {
            if (other == null) return 1;
            if(ReferenceEquals(this, other)) return 0;
            return 1;
        }
    }
}
