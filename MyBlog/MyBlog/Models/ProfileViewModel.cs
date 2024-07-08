

namespace MyBlog.Models
{
    public class ProfileViewModel 
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }  
        public string Username { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public IFormFile? ProfilePicture { get; set; } 

        public List<BlogPost>? BlogPost { get; set; }
    }
}
