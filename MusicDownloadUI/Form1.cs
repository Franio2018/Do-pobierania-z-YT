using DOPOBIERANIAZYT;
using MusicDownloadLib;
using static System.Net.Mime.MediaTypeNames;

namespace MusicDownloadUI
{
    public partial class Form1 : Form
    {
        private DOPOBIERANIAZYT.Test test = new("https://www.youtube.com/channel/UCoRR6OLuIZ2-5VxtnQIaN2w");
        public Form1()
        {
            InitializeComponent();
            SetupUI();
            test.channel.playlists.ForEach(playlist => playlistList.Items.Add(playlist.apiPlaylist.Snippet.Title));
        }

        private void playlistList_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = playlistList.SelectedIndex;
            if (selectedIndex >= 0) 
            {
                Playlist selectedPlaylist = test.channel.GetPlaylist(selectedIndex);
                if (selectedPlaylist != null)
                {
                    videosList.Items.Clear();
                    selectedPlaylist.GetVideos()
                        .ForEach(video => videosList.Items.Add(video.Snippet.Title));
                }
            }
        }

        private void showAllButton_Click(object sender, EventArgs e)
        {
            videosList.Items.Clear();

            foreach (var playlist in test.channel.playlists)
            {
                var videos = playlist.GetVideos();

                foreach (var video in videos)
                {
                    videosList.Items.Add(video.Snippet.Title);
                }
            }
        }
    }
}
