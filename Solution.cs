using System.Text.Json.Serialization;

public class Solution
{
    public int QuestionId { get; set; }
    [JsonIgnore]
    public Question Question { get; set; } = new();
    public int Id { get; set; }
    public int UserId { get; set; }
    public string User { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int UpVotes { get; set; }
    public int DownVotes { get; set; }
    public List<Reply> Replies { get; set; } = new List<Reply>();
}