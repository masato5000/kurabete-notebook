using KurabeteNotebook.Core.Models;

namespace KurabeteNotebook.Core.Services;

public static class LectureCardService
{
    public static string Generate(Case c)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"【ケース #{c.Id}】");
        sb.AppendLine($"結果: {OutcomeLabel(c.Outcome)}");
        sb.AppendLine($"タイムスタンプ: {TimeSpan.FromSeconds(c.TimestampSeconds):hh\\:mm\\:ss}");
        if (!string.IsNullOrWhiteSpace(c.Memo))
            sb.AppendLine($"メモ: {c.Memo}");
        foreach (var f in c.Fields)
            sb.AppendLine($"{f.Key}: {f.Value}");
        return sb.ToString();
    }

    private static string OutcomeLabel(Outcome o) => o switch
    {
        Outcome.Good    => "✓ 良い",
        Outcome.Bad     => "✗ 悪い",
        Outcome.Neutral => "△ 中立",
        _               => "未設定",
    };
}
