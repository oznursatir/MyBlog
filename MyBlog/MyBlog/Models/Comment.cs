namespace MyBlog.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public int BlogPostId { get; set; } // İçeriğe ait ID
        public string UserId { get; set; } // Kullanıcının ID'si

        
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // İlişkiler
        public BlogPost BlogPost { get; set; }
        public CustomUser? User { get; set; }
        public List<EditComment> Comments { get; set; }
    }
}
