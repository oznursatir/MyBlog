using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MyBlog.Models
{
    public class EditUserPageModel
    {
        public EditUserModel User { get; set; }
        public List<SelectListItem> AllRoles { get; set; }
    }
}
