namespace MyBlog.Models
{
    using System.Collections.Generic;

    public class BlogSearchViewModel
    {
        public string SearchTerm { get; set; }
        public List<BlogPost> Results { get; set; }
        
    }
}

