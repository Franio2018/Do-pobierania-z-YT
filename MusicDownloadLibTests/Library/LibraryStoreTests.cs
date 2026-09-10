using Microsoft.Data.Sqlite;

namespace MusicDownloadLib.Tests
{
    [TestClass]
    public class LibraryStoreTests
    {
        [TestMethod]
        public void SetCurrentAccount_IsRemembered()
        {
            var path = Path.Combine(Path.GetTempPath(), $"music-lib-{Guid.NewGuid():N}.db");
            try
            {
                using (var store = new LibraryStore(path))
                {
                    store.SetCurrentAccount("UCaccount1xx123456789012", "Tester", "https://www.youtube.com/channel/UCaccount1xx123456789012");
                }

                using (var store = new LibraryStore(path))
                {
                    var account = store.GetCurrentAccount();
                    Assert.IsNotNull(account);
                    Assert.AreEqual("UCaccount1xx123456789012", account.AccountId);
                    Assert.AreEqual("Tester", account.Name);
                }
            }
            finally
            {
                SqliteConnection.ClearAllPools();
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        [TestMethod]
        public void AddOrIncrementListen_CountsRepeats()
        {
            var path = Path.Combine(Path.GetTempPath(), $"music-lib-{Guid.NewGuid():N}.db");
            try
            {
                using var store = new LibraryStore(path);
                store.AddOrIncrementListen("UCaccount1xx", "aaaaaaaaaaa", "First", "https://youtu.be/aaaaaaaaaaa");
                var second = store.AddOrIncrementListen("UCaccount1xx", "aaaaaaaaaaa", "First", null);

                Assert.AreEqual(2, second.ListenCount);
                Assert.AreEqual(1, store.GetListens("UCaccount1xx").Count);
                Assert.AreEqual(0, store.GetListens("UCotherxxxxx").Count);
            }
            finally
            {
                SqliteConnection.ClearAllPools();
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        [TestMethod]
        public void ImportListens_MergesCountsWithoutDoubling()
        {
            var path = Path.Combine(Path.GetTempPath(), $"music-lib-{Guid.NewGuid():N}.db");
            try
            {
                using var store = new LibraryStore(path);
                store.MergeListen("UCaccount1xx", "dQw4w9WgXcQ", "Song", null, 2);
                store.ImportListens("UCaccount1xx",
                [
                    new ListenExportItem { VideoId = "dQw4w9WgXcQ", Title = "Song", ListenCount = 2 }
                ]);

                var listens = store.GetListens("UCaccount1xx");
                Assert.AreEqual(1, listens.Count);
                Assert.AreEqual(2, listens[0].ListenCount);
            }
            finally
            {
                SqliteConnection.ClearAllPools();
                if (File.Exists(path))
                    File.Delete(path);
            }
        }

        [TestMethod]
        public void GetLatestSuccessfulDownloads_KeepsNewestPerVideo()
        {
            var path = Path.Combine(Path.GetTempPath(), $"music-lib-{Guid.NewGuid():N}.db");
            try
            {
                using var store = new LibraryStore(path);
                store.AddDownload("UCaccount1xx", "aaaaaaaaaaa", "Song", @"C:\old.m4a", "Success");
                store.AddDownload("UCaccount1xx", "aaaaaaaaaaa", "Song", @"C:\new.m4a", "Success");
                store.AddDownload("UCaccount1xx", "bbbbbbbbbbb", "Other", null, "Failed", "nope");
                store.AddDownload("UCotherxxxxx", "aaaaaaaaaaa", "Song", @"C:\other.m4a", "Success");

                var owned = store.GetLatestSuccessfulDownloads("UCaccount1xx");
                Assert.AreEqual(1, owned.Count);
                Assert.AreEqual(@"C:\new.m4a", owned[0].FilePath);
            }
            finally
            {
                SqliteConnection.ClearAllPools();
                if (File.Exists(path))
                    File.Delete(path);
            }
        }
    }

    [TestClass]
    public class ListenFileTests
    {
        [TestMethod]
        public void Parse_ReadsExtensionExport()
        {
            var json = """
                {
                  "version": 1,
                  "listens": [
                    { "videoId": "dQw4w9WgXcQ", "title": "Never Gonna Give You Up", "listenCount": 3 }
                  ]
                }
                """;

            var file = ListenFile.Parse(json);
            Assert.AreEqual(1, file.Listens.Count);
            Assert.AreEqual("dQw4w9WgXcQ", file.Listens[0].VideoId);
            Assert.AreEqual(3, file.Listens[0].ListenCount);
        }

        [TestMethod]
        public void TryDelete_RemovesExistingFile()
        {
            var path = Path.Combine(Path.GetTempPath(), $"pending-listens-{Guid.NewGuid():N}.json");
            File.WriteAllText(path, "{}");
            Assert.IsTrue(ListenFile.TryDelete(path));
            Assert.IsFalse(File.Exists(path));
        }
    }
}
