using X.PagedList;




namespace MyBlog.Models
{
    public class BlogPostCommentsViewModel
    {
        public BlogPost BlogPost { get; set; }
        public IPagedList<Comment> PagedComments { get; set; }
       
    }
}
