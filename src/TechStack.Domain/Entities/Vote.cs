namespace TechStack.Domain.Entities;

public class Vote
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public Post? Post { get; set; }
    public int? UserId { get; set; }
    public User? User { get; set; }
    public int? BountyAmount { get; set; }
    public int VoteTypeId { get; set; }
    public VoteType? VoteType { get; set; }
    public DateTime CreationDate { get; set; }
}
