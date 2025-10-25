using System.Text.Json.Serialization;

public class UserVote
{
    public int UserId { get; set; }
    [JsonIgnore]
    public User User { get; set; } = new();
    public int Id { get; set; }
    public string Vote { get; set; } = string.Empty;
    public int SolutionId { get; set; }
}