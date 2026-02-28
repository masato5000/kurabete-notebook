using KurabeteNotebook.Core.Models;
using Microsoft.Data.Sqlite;

namespace KurabeteNotebook.Data.Repositories;

public class CaseFieldRepository
{
    private readonly SqliteConnection _conn;
    public CaseFieldRepository(SqliteConnection conn) => _conn = conn;

    public IEnumerable<CaseField> GetByCase(int caseId)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = "SELECT id, case_id, key, value FROM case_fields WHERE case_id = $c ORDER BY id";
        cmd.Parameters.AddWithValue("$c", caseId);
        using var r = cmd.ExecuteReader();
        while (r.Read())
            yield return new CaseField
            {
                Id = r.GetInt32(0),
                CaseId = r.GetInt32(1),
                Key = r.GetString(2),
                Value = r.GetString(3),
            };
    }

    public CaseField Upsert(int caseId, string key, string value)
    {
        using var cmd = _conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO case_fields (case_id, key, value) VALUES ($c, $k, $v)
            ON CONFLICT DO NOTHING;
            UPDATE case_fields SET value = $v WHERE case_id = $c AND key = $k;
            SELECT id FROM case_fields WHERE case_id = $c AND key = $k;";
        cmd.Parameters.AddWithValue("$c", caseId);
        cmd.Parameters.AddWithValue("$k", key);
        cmd.Parameters.AddWithValue("$v", value);
        var id = Convert.ToInt32(cmd.ExecuteScalar());
        return new CaseField { Id = id, CaseId = caseId, Key = key, Value = value };
    }
}
