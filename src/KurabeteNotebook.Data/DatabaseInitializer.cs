using Microsoft.Data.Sqlite;

namespace KurabeteNotebook.Data;

public static class DatabaseInitializer
{
    public static string GetDbPath()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "kurabete-notebook");
        Directory.CreateDirectory(folder);
        return Path.Combine(folder, "library.db");
    }

    public static SqliteConnection CreateConnection()
    {
        var conn = new SqliteConnection($"Data Source={GetDbPath()}");
        conn.Open();
        return conn;
    }

    public static void Initialize(SqliteConnection conn)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS videos (
                id           INTEGER PRIMARY KEY AUTOINCREMENT,
                title        TEXT NOT NULL,
                file_path    TEXT NOT NULL,
                registered_at TEXT NOT NULL DEFAULT (datetime('now'))
            );
            CREATE TABLE IF NOT EXISTS cases (
                id               INTEGER PRIMARY KEY AUTOINCREMENT,
                video_id         INTEGER NOT NULL REFERENCES videos(id),
                timestamp_seconds REAL NOT NULL,
                outcome          TEXT NOT NULL DEFAULT 'None',
                memo             TEXT NOT NULL DEFAULT '',
                created_at       TEXT NOT NULL DEFAULT (datetime('now'))
            );
            CREATE TABLE IF NOT EXISTS case_fields (
                id       INTEGER PRIMARY KEY AUTOINCREMENT,
                case_id  INTEGER NOT NULL REFERENCES cases(id) ON DELETE CASCADE,
                key      TEXT NOT NULL,
                value    TEXT NOT NULL DEFAULT ''
            );";
        cmd.ExecuteNonQuery();
    }
}
