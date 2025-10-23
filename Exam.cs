public class Exam
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Course { get; set; } = string.Empty;
    public List<Question> Questions { get; set; } = new List<Question>();
}