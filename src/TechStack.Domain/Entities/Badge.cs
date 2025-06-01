namespace TechStack.Domain.Entities;

public class Badge
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public DateTime Date { get; set; }
}
