namespace MusicDownloadUI
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            messageWindow = new RichTextBox();
            progressBar = new ProgressBar();
            downloadButton = new Button();
            qualityCommboBox = new ComboBox();
            splitContainer1 = new SplitContainer();
            playlistList = new ListBox();
            refreshButton = new Button();
            showAllButton = new Button();
            videosList = new ListView();
            selectAllButton = new Button();
            searchBar = new TextBox();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // messageWindow
            // 
            messageWindow.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            messageWindow.Location = new Point(3, 433);
            messageWindow.Name = "messageWindow";
            messageWindow.ReadOnly = true;
            messageWindow.Size = new Size(795, 60);
            messageWindow.TabIndex = 0;
            messageWindow.Text = "";
            // 
            // progressBar
            // 
            progressBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            progressBar.Location = new Point(268, 395);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(530, 34);
            progressBar.TabIndex = 1;
            // 
            // downloadButton
            // 
            downloadButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            downloadButton.Location = new Point(160, 395);
            downloadButton.Name = "downloadButton";
            downloadButton.Size = new Size(102, 34);
            downloadButton.TabIndex = 2;
            downloadButton.Text = "Download";
            downloadButton.UseVisualStyleBackColor = true;
            // 
            // qualityCommboBox
            // 
            qualityCommboBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            qualityCommboBox.FormattingEnabled = true;
            qualityCommboBox.Location = new Point(3, 399);
            qualityCommboBox.Name = "qualityCommboBox";
            qualityCommboBox.Size = new Size(151, 28);
            qualityCommboBox.TabIndex = 3;
            // 
            // splitContainer1
            // 
            splitContainer1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(playlistList);
            splitContainer1.Panel1.Controls.Add(refreshButton);
            splitContainer1.Panel1.Controls.Add(showAllButton);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(videosList);
            splitContainer1.Panel2.Controls.Add(selectAllButton);
            splitContainer1.Panel2.Controls.Add(searchBar);
            splitContainer1.Size = new Size(798, 389);
            splitContainer1.SplitterDistance = 266;
            splitContainer1.TabIndex = 4;
            // 
            // playlistList
            // 
            playlistList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            playlistList.FormattingEnabled = true;
            playlistList.Location = new Point(4, 95);
            playlistList.Name = "playlistList";
            playlistList.Size = new Size(258, 284);
            playlistList.TabIndex = 2;
            playlistList.SelectedIndexChanged += playlistList_SelectedIndexChanged;
            // 
            // refreshButton
            // 
            refreshButton.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            refreshButton.Location = new Point(4, 49);
            refreshButton.Name = "refreshButton";
            refreshButton.Size = new Size(259, 40);
            refreshButton.TabIndex = 1;
            refreshButton.Text = "Refresh";
            refreshButton.UseVisualStyleBackColor = true;
            // 
            // showAllButton
            // 
            showAllButton.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            showAllButton.Location = new Point(3, 3);
            showAllButton.Name = "showAllButton";
            showAllButton.Size = new Size(259, 40);
            showAllButton.TabIndex = 0;
            showAllButton.Text = "Show All";
            showAllButton.UseVisualStyleBackColor = true;
            showAllButton.Click += showAllButton_Click;
            // 
            // videosList
            // 
            videosList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            videosList.Location = new Point(3, 36);
            videosList.Name = "videosList";
            videosList.Size = new Size(522, 350);
            videosList.TabIndex = 2;
            videosList.UseCompatibleStateImageBehavior = false;
            // 
            // selectAllButton
            // 
            selectAllButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            selectAllButton.Location = new Point(415, 2);
            selectAllButton.Name = "selectAllButton";
            selectAllButton.Size = new Size(110, 29);
            selectAllButton.TabIndex = 1;
            selectAllButton.Text = "Select All";
            selectAllButton.UseVisualStyleBackColor = true;
            // 
            // searchBar
            // 
            searchBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            searchBar.Location = new Point(3, 3);
            searchBar.Name = "searchBar";
            searchBar.Size = new Size(406, 27);
            searchBar.TabIndex = 0;
            searchBar.Text = "\"Filter videos...\"";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 505);
            Controls.Add(splitContainer1);
            Controls.Add(qualityCommboBox);
            Controls.Add(downloadButton);
            Controls.Add(progressBar);
            Controls.Add(messageWindow);
            Name = "Form1";
            Text = "Form1";
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
        }

        private void SetupUI()
        {
            SearchBarSetUp();
            VideosListSetUp();
            QualityCommboBoxSetUp();
            MessageWindowSetUp();
        }

        private void QualityCommboBoxSetUp()
        {
            qualityCommboBox.Items.AddRange(["Audio only", "720p", "1080p"]);
        }

        private void MessageWindowSetUp()
        {
            messageWindow.Dock = DockStyle.Bottom;
            messageWindow.Height = 40;
            messageWindow.Multiline = true;
            messageWindow.ReadOnly = true;
        }

        private void VideosListSetUp()
        {
            videosList.Columns.Add("Thumbnail", 100);
            videosList.Columns.Add("Title", 250);
            videosList.Columns.Add("Duration", 80);
            videosList.Columns.Add("Published", 120);
        }

        private void SearchBarSetUp()
        {
            searchBar.Dock = DockStyle.Top;
            searchBar.ForeColor = Color.Gray;
            searchBar.Text = "Filter videos...";

            searchBar.GotFocus += (s, e) =>
            {
                if (searchBar.ForeColor == Color.Gray)
                {
                    searchBar.Text = "";
                    searchBar.ForeColor = Color.Black;
                }
            };

            searchBar.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(searchBar.Text))
                {
                    searchBar.Text = "Filter videos...";
                    searchBar.ForeColor = Color.Gray;
                }
            };
        }

        #endregion

        private RichTextBox messageWindow;
        private ProgressBar progressBar;
        private Button downloadButton;
        private ComboBox qualityCommboBox;
        private SplitContainer splitContainer1;
        private Button refreshButton;
        private Button showAllButton;
        private ListBox playlistList;
        private TextBox searchBar;
        private Button selectAllButton;
        private ListView videosList;
    }
}
