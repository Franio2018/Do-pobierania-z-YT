using Microsoft.Data.Sqlite;

namespace MusicDownloadLib.Tests
{
    [TestClass]
    public class YouTubeUrlTests
    {
        [TestMethod]
        [DataRow("dQw4w9WgXcQ", "dQw4w9WgXcQ")]
        [DataRow("https://www.youtube.com/watch?v=dQw4w9WgXcQ", "dQw4w9WgXcQ")]
        [DataRow("https://youtu.be/dQw4w9WgXcQ", "dQw4w9WgXcQ")]
        [DataRow("https://www.youtube.com/shorts/dQw4w9WgXcQ", "dQw4w9WgXcQ")]
        [DataRow("https://www.youtube.com/embed/dQw4w9WgXcQ", "dQw4w9WgXcQ")]
        [DataRow("https://www.youtube.com/watch?v=dQw4w9WgXcQ&t=30s", "dQw4w9WgXcQ")]
        public void TryGetVideoId_ParsesCommonFormats(string input, string expected)
        {
            Assert.IsTrue(YouTubeUrl.TryGetVideoId(input, out var videoId));
            Assert.AreEqual(expected, videoId);
        }

        [TestMethod]
        public void TryGetVideoId_RejectsChannelUrl()
        {
            Assert.IsFalse(YouTubeUrl.TryGetVideoId("https://www.youtube.com/@NFmusic", out _));
        }
    }
}
