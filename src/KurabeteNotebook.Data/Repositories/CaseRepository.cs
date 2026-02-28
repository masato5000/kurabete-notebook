using KurabeteNotebook.Core.Models;
using Microsoft.Data.Sqlite;

namespace KurabeteNotebook.Data.Repositories;

public class CaseRepository
{
    private readonly SqliteConnection _conn;
    public CaseRepository(SqliteConnection conn) => _conn = conn;

    public IEnumerable<Case> GetByVideo(int videoId)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = @"
            SELECT id, video_id, timestamp_seconds, outcome, memo, created_at
            FROM cases WHERE video_id = $vid ORDER BY id DESC";
        cmd.Parameters.AddWithValue("$vid", videoId);
        using var r = cmd.ExecuteReader();
        while (r.Read())
            yield return new Case
            {
                Id = r.GetInt32(0),
                VideoId = r.GetInt32(1),
                TimestampSeconds = r.GetDouble(2),
                Outcome = Enum.Parse<Outcome>(r.GetString(3)),
                Memo = r.GetString(4),
                CreatedAt = DateTime.Parse(r.GetString(5)),
            };
    }

    public Case Insert(int videoId, double timestampSeconds)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO cases (video_id, timestamp_seconds) VALUES ($v, $t);
            SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("$v", videoId);
        cmd.Parameters.AddWithValue("$t", timestampSeconds);
        var id = Convert.ToInt32(cmd.ExecuteScalar());
        return new Case { Id = id, VideoId = videoId, TimestampSeconds = timestampSeconds, Outcome = Outcome.None, CreatedAt = DateTime.UtcNow };
    }

    public void UpdateOutcome(int caseId, Outcome outcome)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "UPDATE cases SET outcome = $o WHERE id = $id";
        cmd.Parameters.AddWithValue("$o", outcome.ToString());
        cmd.Parameters.AddWithValue("$id", caseId);
        cmd.ExecuteNonQuery();
    }

    public void UpdateMemo(int caseId, string memo)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "UPDATE cases SET memo = $m WHERE id = $id";
        cmd.Parameters.AddWithValue("$m", memo);
        cmd.Parameters.AddWithValue("$id", caseId);
        cmd.ExecuteNonQuery();
    }
}
