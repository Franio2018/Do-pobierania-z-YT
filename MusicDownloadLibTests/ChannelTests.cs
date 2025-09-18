using System.Threading.Channels;

namespace MusicDownloadLib.Tests
{
    [TestClass()]
    public class ChannelTests
    {
        [TestMethod()]
        public void ChannelTest()
        {
            Channel channel = new Channel("https://www.youtube.com/channel/UCoRR6OLuIZ2-5VxtnQIaN2w", "","");
            Assert.IsNotNull(channel);

            Assert.AreEqual(channel.name, "NFrealmusic");
            Assert.AreEqual(channel.id, "UCoRR6OLuIZ2-5VxtnQIaN2w");

            Channel channel2 = new Channel("", "NFrealmusic", "");
            channel.CompareTo(channel2);
        }

        [TestMethod()]
        public void GetChannelPlaylistsTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void GetPlaylistTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ResolveChannelIdTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void ResolveChannelNameTest()
        {
            Assert.Fail();
        }
    }
}