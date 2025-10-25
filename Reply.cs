using System.Text.Json.Serialization;

public class Reply
{
    public int SolutionId { get; set; }
    [JsonIgnore]
    public Solution Solution { get; set; } = new();
    public int Id { get; set; }
    public string User { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Description { get; set; } = string.Empty;
}