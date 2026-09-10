using Google.Apis.YouTube.v3.Data;
using Channel = MusicDownloadLib.Channels.Channel;
using Image = System.Drawing.Image;

namespace MusicDownloadUI
{
    public partial class Form1 : Form
    {
        private const int RecentlyListenedIndex = 0;
        private const int DownloadHistoryIndex = 1;
        private const int ChannelPlaylistOffset = 2;

        private static readonly HttpClient Http = new();

        private readonly LibraryStore library = new();
        private readonly ListenInboxServer listenInbox = new();
        private Channel? channel;
        private List<PlaylistItem> allVideos = new();
        private int displayGeneration;
        private bool updatingSources;

        public Form1()
        {
            InitializeComponent();
            SetupUI();
            FormClosed += (_, _) =>
            {
                listenInbox.Dispose();
                library.Dispose();
            };
            Shown += async (_, _) =>
            {
                Log("Library: " + LibraryStore.GetDefaultDatabasePath());
                Log("Download folder: " + PlaylistItemExtension.OutputDirectory);
                RestoreAccountField();
                StartListenInbox();
                FillSourceList();
                TryImportPendingFiles();
                await LoadChannelAsync();
            };
        }

        private void FillSourceList()
        {
            updatingSources = true;
            try
            {
                var selected = playlistList.SelectedIndex;
                playlistList.Items.Clear();
                playlistList.Items.Add("Recently listened");
                playlistList.Items.Add("Download history");
                if (channel is not null)
                {
                    foreach (var playlist in channel.playlists)
                        playlistList.Items.Add(playlist.GetTitle());
                }

                if (selected >= 0 && selected < playlistList.Items.Count)
                    playlistList.SelectedIndex = selected;
            }
            finally
            {
                updatingSources = false;
            }
        }

        private async void refreshButton_Click(object? sender, EventArgs e)
        {
            await LoadChannelAsync();
        }

        private void RestoreAccountField()
        {
            var account = library.GetCurrentAccount();
            if (account is null)
                return;

            listenUrlTextBox.Text = account.Url ?? account.AccountId;
            Log($"Account: {account.Name} ({account.AccountId})");
        }

        private void StartListenInbox()
        {
            listenInbox.ListenReceived += item =>
            {
                if (IsDisposed)
                    return;
                BeginInvoke(() => ApplyIncomingListen(item));
            };
            listenInbox.ListensReceived += items =>
            {
                if (IsDisposed)
                    return;
                BeginInvoke(() => ApplyIncomingListens(items));
            };

            try
            {
                listenInbox.Start();
                Log($"Listen inbox: http://127.0.0.1:{ListenInboxServer.Port}/listened");
            }
            catch (Exception ex)
            {
                Log("Could not start listen inbox: " + ex.Message);
            }
        }

        private void ApplyIncomingListen(ListenExportItem item)
        {
            var account = library.GetCurrentAccount();
            if (account is null)
            {
                ListenFile.AppendPending(item);
                Log($"Queued listen (set account to import): {item.Title}");
                return;
            }

            library.AddOrIncrementListen(account.AccountId, item.VideoId, item.Title, item.Url);
            Log($"Listened: {item.Title}");
        }

        private void ApplyIncomingListens(IReadOnlyList<ListenExportItem> items)
        {
            var account = library.GetCurrentAccount();
            if (account is null)
            {
                ListenFile.Save(ListenFile.PendingPath, new ListenExportFile { Listens = items.ToList() });
                Log($"Queued {items.Count} listen(s) (set account to import)");
                return;
            }

            var imported = library.ImportListens(account.AccountId, items);
            Log($"Synced {imported} listen(s) from browser");
            if (playlistList.SelectedIndex == RecentlyListenedIndex)
                _ = ShowListenedAsync();
        }

        private async void importListensButton_Click(object? sender, EventArgs e)
        {
            var accountId = RequireAccountId();
            if (accountId is null)
                return;

            using var dialog = new OpenFileDialog
            {
                Title = "Load listens JSON",
                Filter = "Listen files (*.json)|*.json|All files (*.*)|*.*",
                FileName = "pending-listens.json",
                InitialDirectory = Directory.Exists(Path.GetDirectoryName(ListenFile.PendingPath))
                    ? Path.GetDirectoryName(ListenFile.PendingPath)!
                    : Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            ImportListensFromFile(accountId, dialog.FileName);
            playlistList.SelectedIndex = RecentlyListenedIndex;
            await ShowListenedAsync();
        }

        private void TryImportPendingFiles()
        {
            var accountId = library.GetCurrentAccount()?.AccountId;
            if (accountId is null)
                return;

            foreach (var path in ListenFile.FindPendingFiles())
            {
                if (ImportListensFromFile(accountId, path))
                    ListenFile.TryDelete(path);
            }
        }

        private bool ImportListensFromFile(string accountId, string path)
        {
            try
            {
                var file = ListenFile.Load(path);
                var imported = library.ImportListens(accountId, file.Listens);
                Log($"Imported {imported} listen(s) from {path}");
                return true;
            }
            catch (Exception ex)
            {
                Log($"Could not import {path}: {ex.Message}");
                return false;
            }
        }

        private string? RequireAccountId()
        {
            var account = library.GetCurrentAccount();
            if (account is not null)
                return account.AccountId;

            MessageBox.Show(
                "Set your YouTube account first (channel ID like UCxxxx or @handle).",
                "Account required",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return null;
        }

        private async void addListenButton_Click(object? sender, EventArgs e)
        {
            await SetAccountAsync();
        }

        private async Task SetAccountAsync()
        {
            var input = listenUrlTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(input))
            {
                MessageBox.Show("Enter your channel ID (UCxxxx), @handle, or channel URL.", "Info", MessageBoxButtons.OK);
                return;
            }

            if (YouTubeUrl.TryGetVideoId(input, out _))
            {
                MessageBox.Show(
                    "That looks like a video URL. Enter your channel ID or @handle instead.\nYouTube does not share watch history from an ID — this only identifies whose library this is.",
                    "Info",
                    MessageBoxButtons.OK);
                return;
            }

            SetBusy(true);
            Log($"Setting account: {input}");

            try
            {
                var loaded = await Task.Run(() => new Channel(input));
                var account = library.SetCurrentAccount(loaded.id, loaded.name, loaded.url);
                listenUrlTextBox.Text = account.Url ?? account.AccountId;
                Log($"Account set: {account.Name} ({account.AccountId})");
                TryImportPendingFiles();
            }
            catch (Exception ex)
            {
                Log("Could not set account: " + ex.Message);
                MessageBox.Show(ex.Message, "Could not set account", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SetBusy(false);
            }
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
                FillSourceList();

                allVideos = channel.videos.ToList();
                await ShowVideosAsync(allVideos);
                Log($"Loaded {channel.name}: {channel.playlists.Count} playlists, {channel.videos.Count} uploads.");
            }
            catch (Exception ex)
            {
                channel = null;
                FillSourceList();
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
            if (updatingSources)
                return;

            int selectedIndex = playlistList.SelectedIndex;
            if (selectedIndex < 0)
                return;

            try
            {
                if (selectedIndex == RecentlyListenedIndex)
                {
                    await ShowListenedAsync();
                    return;
                }

                if (selectedIndex == DownloadHistoryIndex)
                {
                    await ShowDownloadHistoryAsync();
                    return;
                }

                if (channel is null)
                    return;

                var selectedPlaylist = channel.GetPlaylist(selectedIndex - ChannelPlaylistOffset);
                allVideos = selectedPlaylist.GetVideos();
                await ShowVideosAsync(allVideos);
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }

        private async Task ShowListenedAsync()
        {
            var accountId = RequireAccountId();
            if (accountId is null)
                return;

            allVideos = library.GetListens(accountId)
                .Select(track => track.ToPlaylistItem())
                .ToList();
            await ShowVideosAsync(allVideos);
            Log($"Recently listened: {allVideos.Count} track(s).");
        }

        private async Task ShowDownloadHistoryAsync()
        {
            var accountId = RequireAccountId();
            if (accountId is null)
                return;

            allVideos = library.GetLatestSuccessfulDownloads(accountId)
                .Select(track => track.ToPlaylistItem())
                .ToList();
            await ShowVideosAsync(allVideos);
            Log($"Download history: {allVideos.Count} track(s).");
        }

        private async void showAllButton_Click(object sender, EventArgs e)
        {
            if (channel is null)
                return;

            allVideos = channel.playlists
                .SelectMany(playlist => playlist.GetVideos())
                .DistinctByVideoId()
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

        private static string? GetThumbnailUrl(PlaylistItem video)
        {
            var fromApi = video.Snippet?.Thumbnails?.Medium?.Url
                ?? video.Snippet?.Thumbnails?.Default__?.Url
                ?? video.Snippet?.Thumbnails?.High?.Url;
            if (!string.IsNullOrEmpty(fromApi))
                return fromApi;

            var videoId = video.GetVideoId();
            return string.IsNullOrEmpty(videoId)
                ? null
                : $"https://i.ytimg.com/vi/{videoId}/mqdefault.jpg";
        }

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
            var selectedItems = videosList.SelectedItems
                .Cast<ListViewItem>()
                .Select(item => item.Tag as PlaylistItem)
                .Where(video => video is not null)
                .Cast<PlaylistItem>()
                .ToList();

            var selectedVideos = selectedItems.DistinctByVideoId();

            if (selectedVideos.Count == 0)
            {
                MessageBox.Show("Select videos first, or click Select All.", "Info", MessageBoxButtons.OK);
                return;
            }

            var accountId = RequireAccountId();
            if (accountId is null)
                return;

            Directory.CreateDirectory(PlaylistItemExtension.OutputDirectory);

            const int maxConcurrentDownloads = 5;
            using var semaphore = new SemaphoreSlim(maxConcurrentDownloads);

            int completed = 0;
            int downloaded = 0;
            int skipped = 0;
            int failed = 0;
            int total = selectedVideos.Count;

            progressBar.Minimum = 0;
            progressBar.Maximum = Math.Max(total, 1);
            progressBar.Value = 0;

            SetBusy(true);
            if (selectedItems.Count != total)
                Log($"Ignored {selectedItems.Count - total} duplicate playlist entries.");
            Log($"Downloading {total} unique file(s) to {PlaylistItemExtension.OutputDirectory}");

            var downloadTasks = selectedVideos.Select(async video =>
            {
                await semaphore.WaitAsync();
                try
                {
                    var result = await video.DownloadAsync();
                    var status = result.WroteNewFile ? "Success" : "Skipped";
                    library.AddDownload(accountId, result.VideoId, result.Title, result.FilePath, status);
                    library.AddOrIncrementListen(accountId, result.VideoId, result.Title, "https://www.youtube.com/watch?v=" + result.VideoId);
                    if (result.WroteNewFile)
                        Interlocked.Increment(ref downloaded);
                    else
                        Interlocked.Increment(ref skipped);
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref failed);
                    var videoId = video.GetVideoId() ?? "";
                    var title = video.Snippet?.Title ?? videoId;
                    library.AddDownload(accountId, videoId, title, null, "Failed", ex.Message);
                    Log($"Error downloading {title}: {ex.Message}");
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
                $"Downloaded: {downloaded}\n" +
                $"Already existed: {skipped}\n" +
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
            addListenButton.Enabled = !busy;
            listenUrlTextBox.Enabled = !busy;
            importListensButton.Enabled = !busy;
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
