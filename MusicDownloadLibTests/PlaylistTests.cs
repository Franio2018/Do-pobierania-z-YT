using Google.Apis.YouTube.v3.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MusicDownloadLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicDownloadLib.Tests
{
    [TestClass]
    public class PlaylistTests
    {
        [TestMethod]
        public void RemoveAll_ShouldClearVideos()
        {
            var playlist = new Playlist(new List<PlaylistItem>
            {
            new PlaylistItem { Id = "v1" },
            new PlaylistItem { Id = "v2" }
            });

            var result = playlist.RemoveAll();

            Assert.IsTrue(result);
            Assert.AreEqual(0, playlist.GetVideos().Count);
        }

        [TestMethod]
        public void RemoveRange_ShouldRemoveSubset()
        {
            var playlist = new Playlist(new List<PlaylistItem>
            {
            new PlaylistItem { Id = "v1" },
            new PlaylistItem { Id = "v2" },
            new PlaylistItem { Id = "v3" }
            });

            var result = playlist.RemoveRange(1, 1);

            Assert.IsTrue(result);
            var videos = playlist.GetVideos();
            Assert.AreEqual(2, videos.Count);
            Assert.AreEqual("v1", videos[0].Id);
            Assert.AreEqual("v3", videos[1].Id);
        }

        [TestMethod]
        public void RemoveVideoById_ShouldRemoveMatchingVideo()
        {
            var playlist = new Playlist(new List<PlaylistItem>
            {
            new PlaylistItem { Id = "v1" },
            new PlaylistItem { Id = "v2" }
            });

            var result = playlist.RemoveVideoById("v2");

            Assert.IsTrue(result);
            Assert.AreEqual(1, playlist.GetVideos().Count);
            Assert.AreEqual("v1", playlist.GetVideos()[0].Id);
        }

        [TestMethod]
        public void GetVideo_ShouldReturnCorrectVideo()
        {
            var playlist = new Playlist(new List<PlaylistItem>
            {
            new PlaylistItem { Id = "v1" }
            });

            var video = playlist.GetVideo(0);

            Assert.IsNotNull(video);
            Assert.AreEqual("v1", video.Id);
        }

        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void GetVideo_WithInvalidIndex_ShouldThrow()
        {
            var playlist = new Playlist(new List<PlaylistItem>());
            playlist.GetVideo(0);
        }
    }

}