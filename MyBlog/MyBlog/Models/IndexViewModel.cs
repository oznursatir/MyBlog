namespace MyBlog.Models
{
    public class IndexViewModel
    {
        public List<UserViewModel> Users { get; set; }
        public List<BlogPost> BlogPost { get; set; } 
        public List<Comment> comments { get; set; } 
    }
}
