using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using YoutubeExplode;

namespace MusicDownloadLib
{
    public class MusicGathering
    {
        private YouTubeService youtubeService = new YouTubeService(new BaseClientService.Initializer() { ApiKey = "AIzaSyADJG2lPWphTWLQDcyG9iBjoQmawmtc0uE" });

        public List<(string Title, string Url)> GetAllPlaylistsTitles(string channelId)
        {
            var playlists = new List<(string Title, string Url)>();

            // Request playlists for the channel
            var playlistRequest = youtubeService.Playlists.List("snippet");

            if (!channelId.StartsWith("UC") && channelId.Length != 24)
            {
                channelId = ResolveChannelId(youtubeService, channelId);
            }

            if (!string.IsNullOrEmpty(channelId))
            {
                Console.WriteLine($"Resolved Channel ID: {channelId}");
            }
            else
            {
                Console.WriteLine("Channel ID could not be resolved.");
            }

            playlistRequest.ChannelId = channelId;
            playlistRequest.MaxResults = 50; // Fetch up to 50 playlists at a time

            var nextPageToken = "";
            do
            {
                playlistRequest.PageToken = nextPageToken;

                // Execute the request
                var playlistResponse = playlistRequest.Execute();

                foreach (var playlist in playlistResponse.Items)
                {
                    // Construct the playlist URL
                    string playlistId = playlist.Id;
                    string playlistTitle = playlist.Snippet.Title;
                    string playlistUrl = $"{playlistId}";

                    playlists.Add((playlistTitle, playlistUrl));
                }

                // Update the nextPageToken to fetch more pages
                nextPageToken = playlistResponse.NextPageToken;

            } while (!string.IsNullOrEmpty(nextPageToken));

            return playlists;
        }

        static string ResolveChannelId(YouTubeService youtubeService, string input)
        {
            string channelId = null;

            // Handle full URL inputs
            if (input.Contains("youtube.com"))
            {
                if (input.Contains("/channel/"))
                {
                    // Extract the channel ID directly
                    var parts = input.Split('/');
                    channelId = parts[^1]; // Get the last part of the URL
                }
                else
                {
                    // Use the Search API for custom/legacy URLs
                    var searchRequest = youtubeService.Search.List("snippet");
                    searchRequest.Q = input;
                    searchRequest.Type = "channel";
                    searchRequest.MaxResults = 1;

                    var searchResponse = searchRequest.Execute();
                    foreach (var result in searchResponse.Items)
                    {
                        channelId = result.Snippet.ChannelId;
                        break;
                    }
                }
            }
            else
            {
                // Treat the input as a plain channel name
                var searchRequest = youtubeService.Search.List("snippet");
                searchRequest.Q = input;
                searchRequest.Type = "channel";
                searchRequest.MaxResults = 10;

                var searchResponse = searchRequest.Execute();
                channelId = searchResponse.Items[0].Snippet.ChannelId;
            }
            return channelId;
        }

        public List<(string Title, string Url)> GetMusicFromPlaylist(string uploadsListId)
        {
            var playlistRequest = youtubeService.PlaylistItems.List("snippet");
            playlistRequest.PlaylistId = uploadsListId;
            playlistRequest.MaxResults = 50;
            var musicList = new List<(string Title, string Url)>(); // List to store music titles and URLs

            try
            {
                // Execute the playlist request
                var PlaylistResponse = playlistRequest.Execute();

                // Request playlist items
                var playlistItemsListRequest = youtubeService.PlaylistItems.List("snippet");
                playlistItemsListRequest.PlaylistId = uploadsListId;
                playlistItemsListRequest.MaxResults = 50; // Fetch up to 50 items in one request

                // Execute the request and get the response
                var playlistItemsListResponse = playlistItemsListRequest.Execute();

                // Loop through the playlist items
                foreach (var playlistItem in playlistItemsListResponse.Items)
                {
                    try
                    {
                        // Construct the video URL and get the title
                        string url = "https://www.youtube.com/watch?v=" + playlistItem.Snippet.ResourceId.VideoId;
                        string title = playlistItem.Snippet.Title;

                        // Add the music to the list
                        musicList.Add((title, url));
                    }
                    catch (Exception ex)
                    {
                        // Log an error for this specific item
                        Console.WriteLine($"Error processing playlist item: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log or display the error for the entire method
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            return musicList; // Return the list of music titles and URLs
        }

        public void AddVideoToList(List<(string Title, string Url)> Songs, string Url)
        {
            try
            {
                // Create the Videos request
                var videoRequest = youtubeService.Videos.List("snippet");
                Url = Url.Split("?v=")[^1];
                videoRequest.Id = Url;

                // Execute the request
                var response = videoRequest.Execute();

                // Extract and return the title
                var video = response.Items.FirstOrDefault();
                Songs.Add((video?.Snippet.Title ?? "Title not found", Url));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        public string getYouTubeThumbnail(string YoutubeUrl)
        {
            string youTubeThumb = string.Empty;
            if (YoutubeUrl == "")
                return "";
            if (YoutubeUrl.IndexOf("=") > 0)
            {
                youTubeThumb = YoutubeUrl.Split('=')[1];
                YoutubeUrl = youTubeThumb;
                int i = YoutubeUrl.IndexOf('/');
            }
            if (youTubeThumb.IndexOf("&list") > 0)
            {
                youTubeThumb = YoutubeUrl.Split('&')[0];
            }
            else if (YoutubeUrl.IndexOf("/v/") > 0)
            {
                string strVideoCode = YoutubeUrl.Substring(YoutubeUrl.IndexOf("/v/") + 3);
                int ind = strVideoCode.IndexOf("?");
                youTubeThumb = strVideoCode.Substring(0, ind == -1 ? strVideoCode.Length : ind);
            }
            else if (YoutubeUrl.IndexOf('/') == -1)
            {
                return "http://img.youtube.com/vi/" + youTubeThumb + "/mqdefault.jpg";
            }
            else if (YoutubeUrl.IndexOf('/') < 6)
            {
                youTubeThumb = YoutubeUrl.Split('/')[3];
            }
            else if (YoutubeUrl.IndexOf('/') > 6)
            {
                youTubeThumb = YoutubeUrl.Split('/')[1];
            }
            return "http://img.youtube.com/vi/" + youTubeThumb + "/mqdefault.jpg";
        }
    }
}
