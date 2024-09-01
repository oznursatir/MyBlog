namespace MyBlog.Models
{
    public class AddComment
    {
        public int BlogPostId { get; set; }
        public string UserId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
