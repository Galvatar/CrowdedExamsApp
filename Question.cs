using System.Text.Json.Serialization;

public class Question
{
    public int ExamId { get; set; }
    [JsonIgnore]
    public Exam Exam { get; set; } = new();
    public int Id { get; set; }
    public int Number { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Points { get; set; }
    public List<Solution> Solutions { get; set; } = new List<Solution>();
}