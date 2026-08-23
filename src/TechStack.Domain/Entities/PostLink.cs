namespace TechStack.Domain.Entities;

public class PostLink
{
    public int Id { get; set; }
    public DateTime CreationDate { get; set; }
    public int PostId { get; set; }
    public Post? Post { get; set; }
    public int RelatedPostId { get; set; }
    public Post? RelatedPost { get; set; }
    public int LinkTypeId { get; set; }
    public LinkType? LinkType { get; set; }
}
