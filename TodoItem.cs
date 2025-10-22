public class TodoItem
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
}