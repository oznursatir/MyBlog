namespace MyBlog.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class BlogPost
    {
    public int Id { get; set; }

        [Required(ErrorMessage = "Başlık gereklidir.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "İçerik gereklidir.")]
        public string Content { get; set; }


        public string Author { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public List<Comment> Comments { get; set; } = new List<Comment>();
    }
}

