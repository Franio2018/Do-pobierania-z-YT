using Google.Apis.YouTube.v3.Data;

namespace MusicDownloadLib
{
    public class Channel : IChannel, IComparable<Channel>
    {
        public string url { get; private set; }
        public string name { get; private set; }
        public string id { get; private set; }
        public List<Playlist> playlists { get; private set; } = new();
        public List<PlaylistItem> videos { get; private set; } = new();

        public Channel(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url), "Link to channel is required.");

            this.url = url.Trim();
            name = string.Empty;
            id = string.Empty;

            if (!ResolveChannelId())
                throw new InvalidOperationException("Couldn't resolve channel's ID.");
            if (!ResolveChannelName())
                throw new InvalidOperationException("Couldn't resolve channel's name.");

            FetchPlaylists();
            FetchVideos();
        }

        public bool FetchPlaylists()
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id), "id cannot be null or empty.");

            playlists.Clear();

            var playlistRequest = YouTubeApi.Client.Playlists.List("snippet");
            playlistRequest.ChannelId = id;
            playlistRequest.MaxResults = 50;

            string? nextPageToken = null;
            do
            {
                playlistRequest.PageToken = nextPageToken;
                var playlistResponse = playlistRequest.Execute();

                if (playlistResponse.Items != null)
                    playlists.AddRange(playlistResponse.Items.Select(p => new Playlist(p)));

                nextPageToken = playlistResponse.NextPageToken;
            } while (!string.IsNullOrEmpty(nextPageToken));

            return true;
        }

        public bool FetchVideos()
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id), "id cannot be null or empty.");

            videos.Clear();

            var channelRequest = YouTubeApi.Client.Channels.List("contentDetails");
            channelRequest.Id = id;
            var channelResponse = channelRequest.Execute();

            if (channelResponse.Items == null || channelResponse.Items.Count == 0)
                return false;

            var uploadsPlaylistId = channelResponse.Items[0].ContentDetails?.RelatedPlaylists?.Uploads;
            if (string.IsNullOrEmpty(uploadsPlaylistId))
                return false;

            var playlistItemsRequest = YouTubeApi.Client.PlaylistItems.List("snippet");
            playlistItemsRequest.PlaylistId = uploadsPlaylistId;
            playlistItemsRequest.MaxResults = 50;

            string? nextPageToken = null;
            do
            {
                playlistItemsRequest.PageToken = nextPageToken;
                var playlistItemsResponse = playlistItemsRequest.Execute();

                if (playlistItemsResponse.Items != null)
                    videos.AddRange(playlistItemsResponse.Items);

                nextPageToken = playlistItemsResponse.NextPageToken;
            }
            while (!string.IsNullOrEmpty(nextPageToken));

            return true;
        }

        public Playlist GetPlaylist(int index)
        {
            if (index < 0 || index >= playlists.Count)
                throw new IndexOutOfRangeException();
            return playlists[index];
        }

        public bool ResolveChannelId()
        {
            var input = url.Trim();

            if (LooksLikeChannelId(input))
            {
                id = input;
                return true;
            }

            if (TryGetAbsoluteUri(input, out var uri))
            {
                var segments = uri.AbsolutePath
                    .Split('/', StringSplitOptions.RemoveEmptyEntries);

                var channelIndex = IndexOfSegment(segments, "channel");
                if (channelIndex >= 0 && channelIndex + 1 < segments.Length)
                {
                    id = segments[channelIndex + 1];
                    return !string.IsNullOrEmpty(id);
                }

                var handleSegment = segments.FirstOrDefault(s => s.StartsWith('@'));
                if (handleSegment != null)
                    return TryResolveByHandle(handleSegment.TrimStart('@'));

                var userIndex = IndexOfSegment(segments, "user");
                if (userIndex >= 0 && userIndex + 1 < segments.Length)
                    return TryResolveByUsername(segments[userIndex + 1]);

                var customIndex = IndexOfSegment(segments, "c");
                if (customIndex >= 0 && customIndex + 1 < segments.Length)
                    return TryResolveBySearch(segments[customIndex + 1]);

                return TryResolveBySearch(input);
            }

            if (input.StartsWith('@'))
                return TryResolveByHandle(input.TrimStart('@'));

            return TryResolveByHandle(input) || TryResolveBySearch(input);
        }

        public bool ResolveChannelName()
        {
            if (string.IsNullOrEmpty(id))
                return false;

            var request = YouTubeApi.Client.Channels.List("snippet");
            request.Id = id;
            var response = request.Execute();

            if (response.Items == null || response.Items.Count == 0)
                return false;

            name = response.Items[0].Snippet?.Title ?? string.Empty;
            return !string.IsNullOrEmpty(name);
        }

        public int CompareTo(Channel? other)
        {
            if (other is null) return 1;
            if (ReferenceEquals(this, other)) return 0;
            return string.Compare(name, other.name, StringComparison.OrdinalIgnoreCase);
        }

        private static bool LooksLikeChannelId(string value) =>
            value.StartsWith("UC", StringComparison.Ordinal)
            && value.Length >= 24
            && !value.Contains('/');

        private static bool TryGetAbsoluteUri(string value, out Uri uri)
        {
            if (Uri.TryCreate(value, UriKind.Absolute, out uri!))
                return true;
            return Uri.TryCreate("https://" + value, UriKind.Absolute, out uri!);
        }

        private static int IndexOfSegment(string[] segments, string name) =>
            Array.FindIndex(segments, s => s.Equals(name, StringComparison.OrdinalIgnoreCase));

        private bool TryResolveByHandle(string handle)
        {
            if (string.IsNullOrWhiteSpace(handle))
                return false;

            var request = YouTubeApi.Client.Channels.List("snippet");
            request.ForHandle = handle;
            var response = request.Execute();

            if (response.Items != null && response.Items.Count > 0)
            {
                id = response.Items[0].Id;
                return !string.IsNullOrEmpty(id);
            }

            return TryResolveBySearch("@" + handle);
        }

        private bool TryResolveByUsername(string username)
        {
            var request = YouTubeApi.Client.Channels.List("snippet");
            request.ForUsername = username;
            var response = request.Execute();

            if (response.Items != null && response.Items.Count > 0)
            {
                id = response.Items[0].Id;
                return !string.IsNullOrEmpty(id);
            }

            return TryResolveBySearch(username);
        }

        private bool TryResolveBySearch(string query)
        {
            var searchRequest = YouTubeApi.Client.Search.List("snippet");
            searchRequest.Q = query;
            searchRequest.Type = "channel";
            searchRequest.MaxResults = 1;

            var searchResponse = searchRequest.Execute();
            var channelId = searchResponse.Items?.FirstOrDefault()?.Snippet?.ChannelId;
            if (string.IsNullOrEmpty(channelId))
                return false;

            id = channelId;
            return true;
        }
    }
}
