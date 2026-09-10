namespace MusicDownloadLib.Tests
{
    [TestClass()]
    public class ChannelTests
    {
        private static Channel CreateSampleChannel() =>
            new("https://www.youtube.com/channel/UCoRR6OLuIZ2-5VxtnQIaN2w");

        [TestMethod()]
        public void GetChannelPlaylistsTest()
        {
            var channel = CreateSampleChannel();

            Assert.IsNotNull(channel.playlists);
            Assert.IsTrue(channel.playlists.Count > 0, "Channel should have at least one playlist");
        }

        [TestMethod()]
        public void GetPlaylistTest()
        {
            var channel = CreateSampleChannel();
            var playlist = channel.GetPlaylist(0);

            Assert.IsNotNull(playlist);
            Assert.IsInstanceOfType(playlist, typeof(Playlist));
        }

        [TestMethod()]
        public void ResolveChannelIdTest()
        {
            var channel = CreateSampleChannel();
            Assert.AreEqual("UCoRR6OLuIZ2-5VxtnQIaN2w", channel.id);
        }

        [TestMethod()]
        public void ResolveChannelNameTest()
        {
            var channel = CreateSampleChannel();
            Assert.AreEqual("NFrealmusic", channel.name);
        }

        [TestMethod()]
        public void CompareTo_SortsByName()
        {
            var first = CreateSampleChannel();
            var second = CreateSampleChannel();

            Assert.AreEqual(0, first.CompareTo(second));
            Assert.IsTrue(first.CompareTo(null) > 0);
        }
    }
}
