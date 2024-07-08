using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MyBlog.Data;
using MyBlog.Models;
using System.Data;

namespace MyBlog.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<CustomUser> _userManager;
        

        public ProfileController(ApplicationDbContext context, UserManager<CustomUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("Kullanıcı Bulunamadı");
            }

            var blogPosts = await _context.BlogPosts.Where(bp => bp.Author == user.FullName).ToListAsync();

            var viewModel = new ProfileViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                Username = user.UserName,
                ProfilePictureUrl = user.ProfilePictureUrl,
                BlogPost = blogPosts
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string userId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)

            {
                return NotFound("Kullanıcı Bulunamadı");
            }
            var viewModel = new ProfileViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                Username = user.UserName,
                ProfilePictureUrl = user.ProfilePictureUrl
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditAsync(ProfileViewModel Model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("402", "Kayıt İletilemedi");
                //new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }
                return View();
            }

            var cUser = _context.Users.Find(Model.Id);

            if (cUser == null)
            {
                ModelState.AddModelError("404", "Kullanıcı Bulunamadı");
                //new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }
                return View();
            }

            cUser.FullName = Model.FullName;
            cUser.Email = Model.Email;
            cUser.Gender = Model.Gender;
            cUser.DateOfBirth = Model.DateOfBirth;
            cUser.UserName = Model.Username;
           


            if (Model.ProfilePicture != null)
            {
                var filePath = Path.Combine(@"wwwroot/images/profiles", Model.ProfilePicture.FileName);
                
                var directory = Path.GetDirectoryName(filePath);
                if (Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await Model.ProfilePicture.CopyToAsync(stream);
                    
                }
                cUser.ProfilePictureUrl = $"/images/profiles/{Model.ProfilePicture.FileName}";
            }

            var result = await _userManager.UpdateAsync(cUser);
            if (result.Succeeded)
            {
                return RedirectToAction("Index");
                
            }
                

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(User);
            }

        //Parola Değiştirme
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _userManager.UpdateSecurityStampAsync(user);
                return RedirectToAction("Edit");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
        [HttpGet]
        public IActionResult ChangePasswordConfirmation()
        {
            return View();
        }




    }
}




