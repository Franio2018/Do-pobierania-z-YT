using Google.Apis.YouTube.v3.Data;

namespace MusicDownloadLib.Tests
{
    [TestClass]
    public class PlaylistItemExtensionTests
    {
        [TestMethod]
        public void DistinctByVideoId_RemovesDuplicateVideos()
        {
            var videos = new List<PlaylistItem>
            {
                Item("aaa", "First"),
                Item("bbb", "Second"),
                Item("aaa", "First again"),
                Item(null, "Missing id")
            };

            var unique = videos.DistinctByVideoId();

            Assert.AreEqual(2, unique.Count);
            Assert.AreEqual("aaa", unique[0].GetVideoId());
            Assert.AreEqual("bbb", unique[1].GetVideoId());
        }

        [TestMethod]
        public void SanitizeFileName_StripsInvalidCharacters()
        {
            var name = PlaylistItemExtension.SanitizeFileName(@"NF - TURN MY BACK (Audio): live/show?");
            Assert.AreEqual("NF - TURN MY BACK (Audio) liveshow", name);
        }

        [TestMethod]
        public void GetAudioExtension_MapsMp4ToM4a()
        {
            Assert.AreEqual("m4a", PlaylistItemExtension.GetAudioExtension("mp4"));
            Assert.AreEqual("webm", PlaylistItemExtension.GetAudioExtension("webm"));
        }

        private static PlaylistItem Item(string? videoId, string title) =>
            new()
            {
                Snippet = new PlaylistItemSnippet
                {
                    Title = title,
                    ResourceId = videoId is null ? null : new ResourceId { VideoId = videoId }
                }
            };
    }
}
