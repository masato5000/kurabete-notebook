namespace KurabeteNotebook.Core.Models;

public enum Outcome { None, Good, Bad, Neutral }

public class Case
{
    public int Id { get; set; }
    public int VideoId { get; set; }
    public double TimestampSeconds { get; set; }   // center of the clip
    public double ClipStartSeconds => TimestampSeconds - 20;
    public double ClipEndSeconds   => TimestampSeconds + 30;
    public Outcome Outcome { get; set; } = Outcome.None;
    public string Memo { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<CaseField> Fields { get; set; } = new();
}
