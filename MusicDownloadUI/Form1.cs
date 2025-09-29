using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MusicDownloadUI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            // Split na playlisty/filmy
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 40
            };
            Controls.Add(split);

            // === LEWY PANEL ===
            var playlistPanel = new Panel { Dock = DockStyle.Fill };

            var btnShowAll = new Button { Text = "Show All", Dock = DockStyle.Top };
            var btnRefresh = new Button { Text = "Refresh", Dock = DockStyle.Top };
            var listPlaylists = new ListBox { Dock = DockStyle.Fill };

            playlistPanel.Controls.Add(listPlaylists);
            playlistPanel.Controls.Add(btnRefresh);
            playlistPanel.Controls.Add(btnShowAll);

            split.Panel1.Controls.Add(playlistPanel);

            // === PRAWY PANEL ===
            var rightPanel = new Panel { Dock = DockStyle.Fill };

            // Wyszukiwarka
            var txtFilter = new TextBox { 
                Dock = DockStyle.Top,
                ForeColor = Color.Gray,
                Text = "Filter videos..." };

            txtFilter.GotFocus += (s, e) =>
            {
                if (txtFilter.ForeColor == Color.Gray)
                {
                    txtFilter.Text = "";
                    txtFilter.ForeColor = Color.Black;
                }
            };

            txtFilter.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtFilter.Text))
                {
                    txtFilter.Text = "Filter videos...";
                    txtFilter.ForeColor = Color.Gray;
                }
            };

            rightPanel.Controls.Add(txtFilter);

            // Lista filmów
            var videoList = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                CheckBoxes = true,
                FullRowSelect = true
            };
            videoList.Columns.Add("Thumbnail", 100);
            videoList.Columns.Add("Title", 250);
            videoList.Columns.Add("Duration", 80);
            videoList.Columns.Add("Published", 120);
            rightPanel.Controls.Add(videoList);


            split.Panel2.Controls.Add(rightPanel);

            // === DOLNY PANEL ===
            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 80 };

            var cmbQuality = new ComboBox { Dock = DockStyle.Left, Width = 120 };
            cmbQuality.Items.AddRange(new[] { "Audio only", "720p", "1080p" });

            var btnDownload = new Button { Text = "Download", Dock = DockStyle.Left, Width = 120 };
            var progress = new ProgressBar { Dock = DockStyle.Top, Height = 20 };

            var logBox = new TextBox
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                Multiline = true,
                ReadOnly = true
            };

            bottomPanel.Controls.Add(progress);
            bottomPanel.Controls.Add(btnDownload);
            bottomPanel.Controls.Add(cmbQuality);
            bottomPanel.Controls.Add(logBox);

            Controls.Add(bottomPanel);
        }

    }

}
