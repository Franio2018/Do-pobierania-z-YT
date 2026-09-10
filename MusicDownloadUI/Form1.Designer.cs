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
            importListensButton = new Button();
            addListenButton = new Button();
            listenUrlTextBox = new TextBox();
            refreshButton = new Button();
            showAllButton = new Button();
            channelUrlTextBox = new TextBox();
            videosList = new ListView();
            selectAllButton = new Button();
            searchBar = new TextBox();
            folderButton = new Button();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // messageWindow
            // 
            messageWindow.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            messageWindow.Location = new Point(4, 408);
            messageWindow.Name = "messageWindow";
            messageWindow.ReadOnly = true;
            messageWindow.ScrollBars = RichTextBoxScrollBars.Vertical;
            messageWindow.Size = new Size(795, 78);
            messageWindow.TabIndex = 0;
            messageWindow.Text = "";
            // 
            // progressBar
            // 
            progressBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            progressBar.Location = new Point(268, 370);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(530, 32);
            progressBar.TabIndex = 1;
            // 
            // downloadButton
            // 
            downloadButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            downloadButton.Location = new Point(160, 370);
            downloadButton.Name = "downloadButton";
            downloadButton.Size = new Size(102, 34);
            downloadButton.TabIndex = 2;
            downloadButton.Text = "Download";
            downloadButton.UseVisualStyleBackColor = true;
            downloadButton.Click += downloadButton_Click;
            // 
            // qualityCommboBox
            // 
            qualityCommboBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            qualityCommboBox.FormattingEnabled = true;
            qualityCommboBox.Location = new Point(76, 374);
            qualityCommboBox.Name = "qualityCommboBox";
            qualityCommboBox.Size = new Size(78, 28);
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
            splitContainer1.Panel1.Controls.Add(importListensButton);
            splitContainer1.Panel1.Controls.Add(addListenButton);
            splitContainer1.Panel1.Controls.Add(listenUrlTextBox);
            splitContainer1.Panel1.Controls.Add(refreshButton);
            splitContainer1.Panel1.Controls.Add(showAllButton);
            splitContainer1.Panel1.Controls.Add(channelUrlTextBox);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(videosList);
            splitContainer1.Panel2.Controls.Add(selectAllButton);
            splitContainer1.Panel2.Controls.Add(searchBar);
            splitContainer1.Size = new Size(798, 364);
            splitContainer1.SplitterDistance = 266;
            splitContainer1.TabIndex = 4;
            // 
            // playlistList
            // 
            playlistList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            playlistList.FormattingEnabled = true;
            playlistList.Location = new Point(4, 224);
            playlistList.Name = "playlistList";
            playlistList.Size = new Size(258, 124);
            playlistList.TabIndex = 2;
            playlistList.SelectedIndexChanged += playlistList_SelectedIndexChanged;
            // 
            // importListensButton
            // 
            importListensButton.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            importListensButton.Location = new Point(3, 188);
            importListensButton.Name = "importListensButton";
            importListensButton.Size = new Size(259, 32);
            importListensButton.TabIndex = 6;
            importListensButton.Text = "Load listens";
            importListensButton.UseVisualStyleBackColor = true;
            importListensButton.Click += importListensButton_Click;
            // 
            // addListenButton
            // 
            addListenButton.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            addListenButton.Location = new Point(3, 155);
            addListenButton.Name = "addListenButton";
            addListenButton.Size = new Size(259, 32);
            addListenButton.TabIndex = 5;
            addListenButton.Text = "Set account";
            addListenButton.UseVisualStyleBackColor = true;
            addListenButton.Click += addListenButton_Click;
            // 
            // listenUrlTextBox
            // 
            listenUrlTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            listenUrlTextBox.Location = new Point(3, 125);
            listenUrlTextBox.Name = "listenUrlTextBox";
            listenUrlTextBox.PlaceholderText = "Your account ID or @handle";
            listenUrlTextBox.Size = new Size(259, 27);
            listenUrlTextBox.TabIndex = 4;
            // 
            // refreshButton
            // 
            refreshButton.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            refreshButton.Location = new Point(4, 81);
            refreshButton.Name = "refreshButton";
            refreshButton.Size = new Size(259, 40);
            refreshButton.TabIndex = 1;
            refreshButton.Text = "Refresh";
            refreshButton.UseVisualStyleBackColor = true;
            refreshButton.Click += refreshButton_Click;
            // 
            // showAllButton
            // 
            showAllButton.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            showAllButton.Location = new Point(3, 36);
            showAllButton.Name = "showAllButton";
            showAllButton.Size = new Size(259, 40);
            showAllButton.TabIndex = 0;
            showAllButton.Text = "Show All";
            showAllButton.UseVisualStyleBackColor = true;
            showAllButton.Click += showAllButton_Click;
            // 
            // channelUrlTextBox
            // 
            channelUrlTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            channelUrlTextBox.Location = new Point(3, 3);
            channelUrlTextBox.Name = "channelUrlTextBox";
            channelUrlTextBox.PlaceholderText = "Channel URL...";
            channelUrlTextBox.Size = new Size(259, 27);
            channelUrlTextBox.TabIndex = 3;
            channelUrlTextBox.Text = "https://www.youtube.com/@NFmusic";
            // 
            // videosList
            // 
            videosList.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            videosList.Location = new Point(3, 36);
            videosList.Name = "videosList";
            videosList.Size = new Size(522, 325);
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
            selectAllButton.Click += selectAllButton_Click;
            // 
            // searchBar
            // 
            searchBar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            searchBar.Location = new Point(3, 3);
            searchBar.Name = "searchBar";
            searchBar.Size = new Size(406, 27);
            searchBar.TabIndex = 0;
            searchBar.Text = "\"Filter videos...\"";
            searchBar.TextChanged += searchBar_TextChanged;
            // 
            // folderButton
            // 
            folderButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            folderButton.Location = new Point(3, 370);
            folderButton.Name = "folderButton";
            folderButton.Size = new Size(70, 34);
            folderButton.TabIndex = 5;
            folderButton.Text = "Folder";
            folderButton.UseVisualStyleBackColor = true;
            folderButton.Click += folderButton_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 487);
            Controls.Add(splitContainer1);
            Controls.Add(folderButton);
            Controls.Add(qualityCommboBox);
            Controls.Add(downloadButton);
            Controls.Add(progressBar);
            Controls.Add(messageWindow);
            Name = "Form1";
            Text = "Form1";
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
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
            channelUrlTextBox.KeyDown += async (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    await LoadChannelAsync();
                }
            };
            listenUrlTextBox.KeyDown += async (_, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    await SetAccountAsync();
                }
            };
        }

        private void QualityCommboBoxSetUp()
        {
            qualityCommboBox.Items.AddRange(["Audio only", "720p", "1080p"]);
            qualityCommboBox.SelectedIndex = 0;
            qualityCommboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void MessageWindowSetUp()
        {
            messageWindow.Multiline = true;
            messageWindow.ReadOnly = true;
        }

        private void VideosListSetUp()
        {
            videosList.MultiSelect = true;
            videosList.HideSelection = false;
            videosList.Columns.Add("Thumbnail", 100);
            videosList.Columns.Add("Title", 250);
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
        private TextBox channelUrlTextBox = null!;
        private Button folderButton = null!;
        private TextBox listenUrlTextBox = null!;
        private Button addListenButton = null!;
        private Button importListensButton = null!;
    }
}
