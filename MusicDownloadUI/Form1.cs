using Google.Apis.YouTube.v3.Data;
using MusicDownloadLib;
using Channel = MusicDownloadLib.Channel;
using Image = System.Drawing.Image;

namespace MusicDownloadUI
{
    public partial class Form1 : Form
    {
        private static readonly HttpClient Http = new();

        private Channel? channel;
        private List<PlaylistItem> allVideos = new();
        private int displayGeneration;

        public Form1()
        {
            InitializeComponent();
            SetupUI();
            Shown += async (_, _) =>
            {
                Log("Download folder: " + PlaylistItemExtension.OutputDirectory);
                await LoadChannelAsync();
            };
        }

        private async void refreshButton_Click(object? sender, EventArgs e)
        {
            await LoadChannelAsync();
        }

        private async Task LoadChannelAsync()
        {
            var url = channelUrlTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(url) || url == "Channel URL...")
            {
                MessageBox.Show("Enter a YouTube channel URL first.", "Info", MessageBoxButtons.OK);
                return;
            }

            SetBusy(true);
            Log($"Loading channel: {url}");

            try
            {
                var loaded = await Task.Run(() => new Channel(url));
                channel = loaded;

                playlistList.Items.Clear();
                foreach (var playlist in channel.playlists)
                    playlistList.Items.Add(playlist.GetTitle());

                allVideos = channel.videos.ToList();
                await ShowVideosAsync(allVideos);
                Log($"Loaded {channel.name}: {channel.playlists.Count} playlists, {channel.videos.Count} uploads.");
            }
            catch (Exception ex)
            {
                channel = null;
                playlistList.Items.Clear();
                videosList.Items.Clear();
                allVideos.Clear();
                Log("Failed to load channel: " + ex.Message);
                MessageBox.Show(ex.Message, "Could not load channel", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async void playlistList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (channel is null)
                return;

            int selectedIndex = playlistList.SelectedIndex;
            if (selectedIndex < 0)
                return;

            try
            {
                var selectedPlaylist = channel.GetPlaylist(selectedIndex);
                allVideos = selectedPlaylist.GetVideos();
                await ShowVideosAsync(allVideos);
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }

        private async void showAllButton_Click(object sender, EventArgs e)
        {
            if (channel is null)
                return;

            allVideos = channel.playlists
                .SelectMany(playlist => playlist.GetVideos())
                .ToList();

            await ShowVideosAsync(allVideos);
        }

        private async Task ShowVideosAsync(IReadOnlyList<PlaylistItem> videos)
        {
            int generation = Interlocked.Increment(ref displayGeneration);

            videosList.BeginUpdate();
            videosList.Items.Clear();

            var imageList = new ImageList
            {
                ImageSize = new Size(120, 90),
                ColorDepth = ColorDepth.Depth32Bit
            };
            videosList.LargeImageList = imageList;
            videosList.View = View.LargeIcon;
            videosList.MultiSelect = true;

            var items = new List<ListViewItem>(videos.Count);
            foreach (var video in videos)
            {
                var item = new ListViewItem
                {
                    Text = video.Snippet?.Title ?? "(untitled)",
                    Tag = video,
                    ImageIndex = -1
                };
                items.Add(item);
                videosList.Items.Add(item);
            }
            videosList.EndUpdate();

            for (int i = 0; i < videos.Count; i++)
            {
                if (generation != displayGeneration)
                    return;

                var thumbnailUrl = GetThumbnailUrl(videos[i]);
                if (thumbnailUrl is null)
                    continue;

                try
                {
                    var data = await Http.GetByteArrayAsync(thumbnailUrl);
                    using var ms = new MemoryStream(data);
                    var img = Image.FromStream(ms);

                    if (generation != displayGeneration)
                    {
                        img.Dispose();
                        return;
                    }

                    imageList.Images.Add(img);
                    items[i].ImageIndex = imageList.Images.Count - 1;
                }
                catch
                {
                    // Thumbnail is optional.
                }
            }
        }

        private static string? GetThumbnailUrl(PlaylistItem video) =>
            video.Snippet?.Thumbnails?.Medium?.Url
            ?? video.Snippet?.Thumbnails?.Default__?.Url
            ?? video.Snippet?.Thumbnails?.High?.Url;

        private async void searchBar_TextChanged(object sender, EventArgs e)
        {
            string query = searchBar.Text.Trim();

            if (string.IsNullOrEmpty(query) || searchBar.ForeColor == Color.Gray || query == "Filter videos...")
            {
                await ShowVideosAsync(allVideos);
                return;
            }

            var filtered = allVideos
                .Where(v => v.Snippet?.Title?.Contains(query, StringComparison.OrdinalIgnoreCase) == true)
                .ToList();

            await ShowVideosAsync(filtered);
        }

        private void selectAllButton_Click(object? sender, EventArgs e)
        {
            videosList.Focus();
            foreach (ListViewItem item in videosList.Items)
                item.Selected = true;
        }

        private void folderButton_Click(object? sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Choose download folder",
                SelectedPath = Directory.Exists(PlaylistItemExtension.OutputDirectory)
                    ? PlaylistItemExtension.OutputDirectory
                    : Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)
            };

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                PlaylistItemExtension.OutputDirectory = dialog.SelectedPath;
                Log("Download folder: " + PlaylistItemExtension.OutputDirectory);
            }
        }

        private async void downloadButton_Click(object sender, EventArgs e)
        {
            var selectedVideos = videosList.SelectedItems
                .Cast<ListViewItem>()
                .Select(item => item.Tag as PlaylistItem)
                .Where(video => video is not null)
                .Cast<PlaylistItem>()
                .ToList();

            if (selectedVideos.Count == 0)
            {
                MessageBox.Show("Select videos first, or click Select All.", "Info", MessageBoxButtons.OK);
                return;
            }

            Directory.CreateDirectory(PlaylistItemExtension.OutputDirectory);

            const int maxConcurrentDownloads = 5;
            using var semaphore = new SemaphoreSlim(maxConcurrentDownloads);

            int completed = 0;
            int failed = 0;
            int total = selectedVideos.Count;

            progressBar.Minimum = 0;
            progressBar.Maximum = Math.Max(total, 1);
            progressBar.Value = 0;

            SetBusy(true);
            Log($"Downloading {total} file(s) to {PlaylistItemExtension.OutputDirectory}");

            var downloadTasks = selectedVideos.Select(async video =>
            {
                await semaphore.WaitAsync();
                try
                {
                    await video.DownloadAsync();
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref failed);
                    Log($"Error downloading {video.Snippet?.Title}: {ex.Message}");
                }
                finally
                {
                    semaphore.Release();
                    int done = Interlocked.Increment(ref completed);
                    BeginInvoke(() =>
                    {
                        if (done <= progressBar.Maximum)
                            progressBar.Value = done;
                    });
                }
            }).ToList();

            await Task.WhenAll(downloadTasks);
            SetBusy(false);

            MessageBox.Show(
                "Downloads completed!\n" +
                $"Success: {total - failed}\n" +
                $"Fail: {failed}",
                "Info",
                MessageBoxButtons.OK);
        }

        private void SetBusy(bool busy)
        {
            downloadButton.Enabled = !busy;
            refreshButton.Enabled = !busy;
            showAllButton.Enabled = !busy;
            playlistList.Enabled = !busy;
            folderButton.Enabled = !busy;
            UseWaitCursor = busy;
        }

        private void Log(string text)
        {
            if (messageWindow.IsDisposed)
                return;

            if (messageWindow.InvokeRequired)
            {
                messageWindow.BeginInvoke(() => Log(text));
                return;
            }

            messageWindow.AppendText($"[{DateTime.Now:HH:mm:ss}] {text}{Environment.NewLine}");
        }
    }
}
