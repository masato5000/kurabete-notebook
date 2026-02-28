using KurabeteNotebook.Core.Models;
using Microsoft.Data.Sqlite;

namespace KurabeteNotebook.Data.Repositories;

public class VideoRepository
{
    private readonly SqliteConnection _conn;
    public VideoRepository(SqliteConnection conn) => _conn = conn;

    public IEnumerable<Video> GetAll()
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT id, title, file_path, registered_at FROM videos ORDER BY id DESC";
        using var r = cmd.ExecuteReader();
        while (r.Read())
            yield return new Video
            {
                Id = r.GetInt32(0),
                Title = r.GetString(1),
                FilePath = r.GetString(2),
                RegisteredAt = DateTime.Parse(r.GetString(3)),
            };
    }

    public Video Insert(string title, string filePath)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "INSERT INTO videos (title, file_path) VALUES ($t, $p); SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("$t", title);
        cmd.Parameters.AddWithValue("$p", filePath);
        var id = Convert.ToInt32(cmd.ExecuteScalar());
        return new Video { Id = id, Title = title, FilePath = filePath, RegisteredAt = DateTime.UtcNow };
    }
}
