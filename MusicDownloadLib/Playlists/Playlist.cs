using Google.Apis.YouTube.v3.Data;

namespace MusicDownloadLib.Playlists
{
    public class Playlist : IPlaylist
    {
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
            apiPlaylist = new Google.Apis.YouTube.v3.Data.Playlist();
            videos = new List<PlaylistItem>(initialVideos);
        }

        private bool FetchVideos()
        {
            if (string.IsNullOrEmpty(apiPlaylist.Id))
                return false;

            videos.Clear();

            var playlistItemsListRequest = YouTubeApi.Client.PlaylistItems.List("snippet");
            playlistItemsListRequest.PlaylistId = apiPlaylist.Id;
            playlistItemsListRequest.MaxResults = 50;

            try
            {
                string? nextPageToken = null;
                do
                {
                    playlistItemsListRequest.PageToken = nextPageToken;
                    var playlistItemsListResponse = playlistItemsListRequest.Execute();

                    if (playlistItemsListResponse.Items != null)
                        videos.AddRange(playlistItemsListResponse.Items);

                    nextPageToken = playlistItemsListResponse.NextPageToken;
                } while (!string.IsNullOrEmpty(nextPageToken));

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return false;
            }
        }

        public bool RemoveAll()
        {
            videos.Clear();
            return true;
        }

        public bool RemoveRange(int index, int count)
        {
            if (index < 0 || count < 0 || index + count > videos.Count)
                return false;

            videos.RemoveRange(index, count);
            return true;
        }

        public bool RemoveVideo(int index)
        {
            if (index < 0 || index >= videos.Count)
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
            if (index < 0 || index >= videos.Count)
                throw new IndexOutOfRangeException();
            return videos[index];
        }

        public List<PlaylistItem> GetVideos()
        {
            return [.. videos];
        }

        public string GetTitle()
        {
            return apiPlaylist.Snippet?.Title ?? string.Empty;
        }
    }
}
