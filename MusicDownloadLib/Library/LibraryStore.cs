using Microsoft.Data.Sqlite;

namespace MusicDownloadLib.Library
{
    public sealed class LibraryStore : IDisposable
    {
        private readonly SqliteConnection _connection;

        public LibraryStore(string? databasePath = null)
        {
            var path = databasePath ?? GetDefaultDatabasePath();
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            _connection = new SqliteConnection(new SqliteConnectionStringBuilder
            {
                DataSource = path,
                Mode = SqliteOpenMode.ReadWriteCreate
            }.ToString());
            _connection.Open();
            Initialize();
        }

        public static string GetDefaultDatabasePath() =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MusicDownload",
                "library.db");

        public UserAccount SetCurrentAccount(string accountId, string name, string? url)
        {
            var now = DateTimeOffset.UtcNow.ToString("O");
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = """
                    INSERT INTO Users (AccountId, Name, Url, LastUsedAt)
                    VALUES ($id, $name, $url, $now)
                    ON CONFLICT(AccountId) DO UPDATE SET
                        Name = excluded.Name,
                        Url = COALESCE(excluded.Url, Users.Url),
                        LastUsedAt = excluded.LastUsedAt;
                    """;
                command.Parameters.AddWithValue("$id", accountId);
                command.Parameters.AddWithValue("$name", name);
                command.Parameters.AddWithValue("$url", (object?)url ?? DBNull.Value);
                command.Parameters.AddWithValue("$now", now);
                command.ExecuteNonQuery();
            }

            SetSetting("CurrentAccountId", accountId);
            return GetCurrentAccount() ?? throw new InvalidOperationException("Account was not saved.");
        }

        public UserAccount? GetCurrentAccount()
        {
            var accountId = GetSetting("CurrentAccountId");
            if (string.IsNullOrEmpty(accountId))
                return null;

            using var command = _connection.CreateCommand();
            command.CommandText = """
                SELECT AccountId, Name, Url, LastUsedAt
                FROM Users
                WHERE AccountId = $id;
                """;
            command.Parameters.AddWithValue("$id", accountId);
            using var reader = command.ExecuteReader();
            return reader.Read() ? ReadAccount(reader) : null;
        }

        public ListenedTrack AddOrIncrementListen(string accountId, string videoId, string title, string? url = null)
        {
            var now = DateTimeOffset.UtcNow.ToString("O");
            using var command = _connection.CreateCommand();
            command.CommandText = """
                INSERT INTO Listens (AccountId, VideoId, Title, Url, ListenCount, FirstListenedAt, LastListenedAt)
                VALUES ($account, $id, $title, $url, 1, $now, $now)
                ON CONFLICT(AccountId, VideoId) DO UPDATE SET
                    Title = excluded.Title,
                    Url = COALESCE(excluded.Url, Listens.Url),
                    ListenCount = Listens.ListenCount + 1,
                    LastListenedAt = excluded.LastListenedAt;
                """;
            command.Parameters.AddWithValue("$account", accountId);
            command.Parameters.AddWithValue("$id", videoId);
            command.Parameters.AddWithValue("$title", title);
            command.Parameters.AddWithValue("$url", (object?)url ?? DBNull.Value);
            command.Parameters.AddWithValue("$now", now);
            command.ExecuteNonQuery();

            return GetListen(accountId, videoId) ?? throw new InvalidOperationException("Listen was not saved.");
        }

        public ListenedTrack MergeListen(
            string accountId,
            string videoId,
            string title,
            string? url,
            int listenCount,
            DateTimeOffset? listenedAt = null)
        {
            var count = Math.Max(1, listenCount);
            var when = (listenedAt ?? DateTimeOffset.UtcNow).ToString("O");
            using var command = _connection.CreateCommand();
            command.CommandText = """
                INSERT INTO Listens (AccountId, VideoId, Title, Url, ListenCount, FirstListenedAt, LastListenedAt)
                VALUES ($account, $id, $title, $url, $count, $when, $when)
                ON CONFLICT(AccountId, VideoId) DO UPDATE SET
                    Title = excluded.Title,
                    Url = COALESCE(excluded.Url, Listens.Url),
                    ListenCount = MAX(Listens.ListenCount, excluded.ListenCount),
                    LastListenedAt = CASE
                        WHEN excluded.LastListenedAt > Listens.LastListenedAt THEN excluded.LastListenedAt
                        ELSE Listens.LastListenedAt
                    END;
                """;
            command.Parameters.AddWithValue("$account", accountId);
            command.Parameters.AddWithValue("$id", videoId);
            command.Parameters.AddWithValue("$title", title);
            command.Parameters.AddWithValue("$url", (object?)url ?? DBNull.Value);
            command.Parameters.AddWithValue("$count", count);
            command.Parameters.AddWithValue("$when", when);
            command.ExecuteNonQuery();

            return GetListen(accountId, videoId) ?? throw new InvalidOperationException("Listen was not saved.");
        }

        public int ImportListens(string accountId, IEnumerable<ListenExportItem> items)
        {
            int imported = 0;
            foreach (var item in items)
            {
                if (!YouTubeUrl.TryGetVideoId(item.VideoId, out var videoId))
                    continue;

                DateTimeOffset? when = null;
                if (DateTimeOffset.TryParse(item.ListenedAt, out var parsed))
                    when = parsed;

                var title = string.IsNullOrWhiteSpace(item.Title) ? videoId : item.Title.Trim();
                MergeListen(accountId, videoId, title, item.Url, item.ListenCount, when);
                imported++;
            }

            return imported;
        }

        public IReadOnlyList<ListenedTrack> GetListens(string accountId)
        {
            using var command = _connection.CreateCommand();
            command.CommandText = """
                SELECT VideoId, Title, Url, ListenCount, FirstListenedAt, LastListenedAt
                FROM Listens
                WHERE AccountId = $account
                ORDER BY LastListenedAt DESC;
                """;
            command.Parameters.AddWithValue("$account", accountId);

            var result = new List<ListenedTrack>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
                result.Add(ReadListen(reader));
            return result;
        }

        public void AddDownload(string accountId, string videoId, string title, string? filePath, string status, string? error = null)
        {
            using var command = _connection.CreateCommand();
            command.CommandText = """
                INSERT INTO Downloads (AccountId, VideoId, Title, FilePath, Status, Error, DownloadedAt)
                VALUES ($account, $id, $title, $path, $status, $error, $now);
                """;
            command.Parameters.AddWithValue("$account", accountId);
            command.Parameters.AddWithValue("$id", videoId);
            command.Parameters.AddWithValue("$title", title);
            command.Parameters.AddWithValue("$path", (object?)filePath ?? DBNull.Value);
            command.Parameters.AddWithValue("$status", status);
            command.Parameters.AddWithValue("$error", (object?)error ?? DBNull.Value);
            command.Parameters.AddWithValue("$now", DateTimeOffset.UtcNow.ToString("O"));
            command.ExecuteNonQuery();
        }

        public IReadOnlyList<DownloadedTrack> GetLatestSuccessfulDownloads(string accountId)
        {
            using var command = _connection.CreateCommand();
            command.CommandText = """
                SELECT d.VideoId, d.Title, d.FilePath, d.Status, d.Error, d.DownloadedAt
                FROM Downloads d
                INNER JOIN (
                    SELECT VideoId, MAX(DownloadedAt) AS Latest
                    FROM Downloads
                    WHERE AccountId = $account
                      AND Status IN ('Success', 'Skipped')
                    GROUP BY VideoId
                ) latest ON d.VideoId = latest.VideoId AND d.DownloadedAt = latest.Latest
                WHERE d.AccountId = $account
                ORDER BY d.DownloadedAt DESC;
                """;
            command.Parameters.AddWithValue("$account", accountId);

            var result = new List<DownloadedTrack>();
            using var reader = command.ExecuteReader();
            while (reader.Read())
                result.Add(ReadDownload(reader));
            return result;
        }

        public void Dispose()
        {
            var connectionString = _connection.ConnectionString;
            _connection.Dispose();
            using var clearer = new SqliteConnection(connectionString);
            SqliteConnection.ClearPool(clearer);
        }

        private ListenedTrack? GetListen(string accountId, string videoId)
        {
            using var command = _connection.CreateCommand();
            command.CommandText = """
                SELECT VideoId, Title, Url, ListenCount, FirstListenedAt, LastListenedAt
                FROM Listens
                WHERE AccountId = $account AND VideoId = $id;
                """;
            command.Parameters.AddWithValue("$account", accountId);
            command.Parameters.AddWithValue("$id", videoId);
            using var reader = command.ExecuteReader();
            return reader.Read() ? ReadListen(reader) : null;
        }

        private void Initialize()
        {
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = """
                    CREATE TABLE IF NOT EXISTS Users (
                        AccountId TEXT PRIMARY KEY,
                        Name TEXT NOT NULL,
                        Url TEXT,
                        LastUsedAt TEXT NOT NULL
                    );

                    CREATE TABLE IF NOT EXISTS Settings (
                        Key TEXT PRIMARY KEY,
                        Value TEXT NOT NULL
                    );
                    """;
                command.ExecuteNonQuery();
            }

            MigrateListens();
            MigrateDownloads();
        }

        private void MigrateListens()
        {
            if (!TableExists("Listens"))
            {
                Execute("""
                    CREATE TABLE Listens (
                        AccountId TEXT NOT NULL,
                        VideoId TEXT NOT NULL,
                        Title TEXT NOT NULL,
                        Url TEXT,
                        ListenCount INTEGER NOT NULL DEFAULT 1,
                        FirstListenedAt TEXT NOT NULL,
                        LastListenedAt TEXT NOT NULL,
                        PRIMARY KEY (AccountId, VideoId)
                    );
                    """);
                return;
            }

            if (ColumnExists("Listens", "AccountId"))
                return;

            Execute("""
                CREATE TABLE Listens_new (
                    AccountId TEXT NOT NULL,
                    VideoId TEXT NOT NULL,
                    Title TEXT NOT NULL,
                    Url TEXT,
                    ListenCount INTEGER NOT NULL DEFAULT 1,
                    FirstListenedAt TEXT NOT NULL,
                    LastListenedAt TEXT NOT NULL,
                    PRIMARY KEY (AccountId, VideoId)
                );
                INSERT INTO Listens_new (AccountId, VideoId, Title, Url, ListenCount, FirstListenedAt, LastListenedAt)
                SELECT 'legacy', VideoId, Title, Url, ListenCount, FirstListenedAt, LastListenedAt FROM Listens;
                DROP TABLE Listens;
                ALTER TABLE Listens_new RENAME TO Listens;
                """);
        }

        private void MigrateDownloads()
        {
            if (!TableExists("Downloads"))
            {
                Execute("""
                    CREATE TABLE Downloads (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        AccountId TEXT NOT NULL,
                        VideoId TEXT NOT NULL,
                        Title TEXT NOT NULL,
                        FilePath TEXT,
                        Status TEXT NOT NULL,
                        Error TEXT,
                        DownloadedAt TEXT NOT NULL
                    );
                    """);
                return;
            }

            if (ColumnExists("Downloads", "AccountId"))
                return;

            Execute("""
                CREATE TABLE Downloads_new (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    AccountId TEXT NOT NULL,
                    VideoId TEXT NOT NULL,
                    Title TEXT NOT NULL,
                    FilePath TEXT,
                    Status TEXT NOT NULL,
                    Error TEXT,
                    DownloadedAt TEXT NOT NULL
                );
                INSERT INTO Downloads_new (AccountId, VideoId, Title, FilePath, Status, Error, DownloadedAt)
                SELECT 'legacy', VideoId, Title, FilePath, Status, Error, DownloadedAt FROM Downloads;
                DROP TABLE Downloads;
                ALTER TABLE Downloads_new RENAME TO Downloads;
                """);
        }

        private bool TableExists(string tableName)
        {
            using var command = _connection.CreateCommand();
            command.CommandText = "SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = $name;";
            command.Parameters.AddWithValue("$name", tableName);
            return command.ExecuteScalar() is not null;
        }

        private bool ColumnExists(string tableName, string columnName)
        {
            using var command = _connection.CreateCommand();
            command.CommandText = $"PRAGMA table_info({tableName});";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (reader.GetString(1).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private void Execute(string sql)
        {
            using var command = _connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }

        private string? GetSetting(string key)
        {
            using var command = _connection.CreateCommand();
            command.CommandText = "SELECT Value FROM Settings WHERE Key = $key;";
            command.Parameters.AddWithValue("$key", key);
            return command.ExecuteScalar() as string;
        }

        private void SetSetting(string key, string value)
        {
            using var command = _connection.CreateCommand();
            command.CommandText = """
                INSERT INTO Settings (Key, Value) VALUES ($key, $value)
                ON CONFLICT(Key) DO UPDATE SET Value = excluded.Value;
                """;
            command.Parameters.AddWithValue("$key", key);
            command.Parameters.AddWithValue("$value", value);
            command.ExecuteNonQuery();
        }

        private static UserAccount ReadAccount(SqliteDataReader reader) =>
            new(
                reader.GetString(0),
                reader.GetString(1),
                reader.IsDBNull(2) ? null : reader.GetString(2),
                DateTimeOffset.Parse(reader.GetString(3)));

        private static ListenedTrack ReadListen(SqliteDataReader reader) =>
            new(
                reader.GetString(0),
                reader.GetString(1),
                reader.IsDBNull(2) ? null : reader.GetString(2),
                reader.GetInt32(3),
                DateTimeOffset.Parse(reader.GetString(4)),
                DateTimeOffset.Parse(reader.GetString(5)));

        private static DownloadedTrack ReadDownload(SqliteDataReader reader) =>
            new(
                reader.GetString(0),
                reader.GetString(1),
                reader.IsDBNull(2) ? null : reader.GetString(2),
                reader.GetString(3),
                reader.IsDBNull(4) ? null : reader.GetString(4),
                DateTimeOffset.Parse(reader.GetString(5)));
    }
}
