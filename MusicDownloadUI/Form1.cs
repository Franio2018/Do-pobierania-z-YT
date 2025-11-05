using DOPOBIERANIAZYT;
using MusicDownloadLib;
using System.Diagnostics.Contracts;
using System.Net;
using System.Security.Cryptography;
using Image = System.Drawing.Image;

namespace MusicDownloadUI
{
    public partial class Form1 : Form
    {
        private List<Google.Apis.YouTube.v3.Data.PlaylistItem> allVideos = new();

        private DOPOBIERANIAZYT.Test test = new("https://www.youtube.com/channel/UCoRR6OLuIZ2-5VxtnQIaN2w");
        public Form1()
        {
            InitializeComponent();
            SetupUI();
            test.channel.playlists.ForEach(playlist => playlistList.Items.Add(playlist.GetTitle()));
        }

        private void playlistList_SelectedIndexChanged(object sender, EventArgs e)
        {
            videosList.Items.Clear();
            int selectedIndex = playlistList.SelectedIndex;
            if (selectedIndex >= 0)
            {
                Playlist selectedPlaylist = test.channel.GetPlaylist(selectedIndex);
                if (selectedPlaylist != null)
                {
                    ShowVideosInList(selectedPlaylist);
                }
            }
        }

        private void showAllButton_Click(object sender, EventArgs e)
        {
            videosList.Items.Clear();

            foreach (var playlist in test.channel.playlists)
            {
                ShowVideosInList(playlist);
            }
        }

        private void ShowVideosInList(IPlaylist playlist)
        {
            ImageList imageList = new ImageList();
            imageList.ImageSize = new Size(120, 90);
            videosList.LargeImageList = imageList;
            videosList.View = View.LargeIcon;

            allVideos = playlist.GetVideos(); // store original videos

            foreach (var video in playlist.GetVideos())
            {
                using (var client = new WebClient())
                {
                    var data = client.DownloadData(video.Snippet.Thumbnails.Default__.Url);
                    using (var ms = new MemoryStream(data))
                    {
                        Image img = Image.FromStream(ms);
                        imageList.Images.Add(img);
                    }
                }

                var item = new ListViewItem
                {
                    ImageIndex = imageList.Images.Count - 1,
                    Text = video.Snippet.Title
                };

                videosList.Items.Add(item);
            }
        }

        private void searchBar_TextChanged(object sender, EventArgs e)
        {
            string query = searchBar.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(query) || query == "filter videos...")
            {
                // Show all
                ShowFilteredVideos(allVideos);
            }
            else
            {
                var filtered = allVideos
                    .Where(v => v.Snippet.Title.ToLower().Contains(query))
                    .ToList();

                ShowFilteredVideos(filtered);
            }
        }

        private void ShowFilteredVideos(List<Google.Apis.YouTube.v3.Data.PlaylistItem> videos)
        {
            videosList.Items.Clear();

            ImageList imageList = new ImageList();
            imageList.ImageSize = new Size(120, 90);
            videosList.LargeImageList = imageList;
            videosList.View = View.LargeIcon;

            foreach (var video in videos)
            {
                using (var client = new WebClient())
                {
                    var data = client.DownloadData(video.Snippet.Thumbnails.Default__.Url);
                    using (var ms = new MemoryStream(data))
                    {
                        Image img = Image.FromStream(ms);
                        imageList.Images.Add(img);
                    }
                }

                var item = new ListViewItem
                {
                    ImageIndex = imageList.Images.Count - 1,
                    Text = video.Snippet.Title
                };

                videosList.Items.Add(item);
            }
        }

        private async void downloadButton_Click(object sender, EventArgs e)
        {
            if (allVideos == null || allVideos.Count == 0)
                return;

            int MAX_CONCURRENT_DOWNLOADS = Environment.ProcessorCount;
            var semaphore = new SemaphoreSlim(MAX_CONCURRENT_DOWNLOADS);

            int completed = 0;
            int total = allVideos.Count;

            progressBar.Minimum = 0;
            progressBar.Maximum = total;
            progressBar.Value = 0;

            int failed = 0;

            var downloadTasks = allVideos.Select(async video =>
            {
                await semaphore.WaitAsync();
                try
                {
                    messageWindow.Text += await video.DownloadAsync(); // your async download method
                }
                catch (Exception ex)
                {
                    // Optional: handle per-video errors
                    Console.WriteLine($"Error downloading {video.Snippet.Title}: {ex.Message}");
                    failed++;
                }
                finally
                {
                    semaphore.Release();
                    Interlocked.Increment(ref completed);
                    progressBar.Invoke((Action)(() => progressBar.Value = completed));
                }
            }).ToList();

            await Task.WhenAll(downloadTasks);

            MessageBox.Show("Downloads completed!\n" +
                $"Success: {allVideos.Count-failed}\n" +
                $"Fail: {failed}"
                ,"Info",MessageBoxButtons.OK);
        }
    }
}
