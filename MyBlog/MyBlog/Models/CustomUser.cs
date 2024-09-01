using Microsoft.AspNetCore.Identity;

using System.ComponentModel.DataAnnotations;

namespace MyBlog.Models
{
    public class CustomUser : IdentityUser
    {

        [Display(Name = "Adı & Soyadı")]
        public string FullName { get; set; }

        
        [Display(Name = "Cinsiyet")]
        public string Gender { get; set; }


        [Display(Name = "Doğum Tarihi")]
        [DataType(DataType.DateTime)]
        public DateTime DateOfBirth { get; set; }

        public string? ProfilePictureUrl { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();

    }
}
