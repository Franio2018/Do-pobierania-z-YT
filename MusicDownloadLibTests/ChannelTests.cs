using System.Threading.Channels;

namespace MusicDownloadLib.Tests
{
    [TestClass()]
    public class ChannelTests
    {
        [TestMethod()]
        public void GetChannelPlaylistsTest()
        {
            var channel = new Channel("https://www.youtube.com/channel/UCoRR6OLuIZ2-5VxtnQIaN2w", "", "");
            var result = channel.GetChannelPlaylists();

            Assert.IsTrue(result);
            Assert.IsNotNull(channel.playlists);
            Assert.IsTrue(channel.playlists.Count > 0, "Channel should have at least one playlist");
        }

        [TestMethod()]
        public void GetPlaylistTest()
        {
            var channel = new Channel("https://www.youtube.com/channel/UCoRR6OLuIZ2-5VxtnQIaN2w", "", "");
            channel.GetChannelPlaylists();

            var playlist = channel.GetPlaylist(0);

            Assert.IsNotNull(playlist);
            Assert.IsInstanceOfType(playlist, typeof(Playlist));
        }

        [TestMethod()]
        public void ResolveChannelIdTest()
        {
            var channel = new Channel("https://www.youtube.com/channel/UCoRR6OLuIZ2-5VxtnQIaN2w", "", "");
            Assert.AreEqual("UCoRR6OLuIZ2-5VxtnQIaN2w", channel.id);
        }

        [TestMethod()]
        public void ResolveChannelNameTest()
        {
            var channel = new Channel("https://www.youtube.com/channel/UCoRR6OLuIZ2-5VxtnQIaN2w", "", "");
            Assert.AreEqual("NFrealmusic", channel.name);
        }

    }
}