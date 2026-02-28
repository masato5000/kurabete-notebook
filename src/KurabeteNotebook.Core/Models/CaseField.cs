namespace KurabeteNotebook.Core.Models;

public class CaseField
{
    public int Id { get; set; }
    public int CaseId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
