using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using MyBlog.Data;
using MyBlog.Data.Migrations;
using MyBlog.Models;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Security.Claims;
using X.PagedList;

namespace MyBlog.Controllers
{
    [Authorize(Roles = "Admin ")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<CustomUser> _userManager;
        public UserController(ApplicationDbContext context, UserManager<CustomUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> LoadUsers()
        {
            var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();

            var users = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(searchValue))
            {
                var normalizedSearch = searchValue.ToLower().Replace(" ", "");
                users = users.Where(u => u.UserName.ToLower().Contains(normalizedSearch) ||
                                         u.Email.ToLower().Contains(normalizedSearch) ||
                                         u.FullName.ToLower().Contains(normalizedSearch));
            }

            var totalRecords = await users.CountAsync();
            var userData = await users.Skip(int.Parse(start)).Take(int.Parse(length)).ToListAsync();

            var roles = new List<string>();
            foreach (var user in userData)
            {
                roles.Add(string.Join(", ", await _userManager.GetRolesAsync(user)));
            }

            var jsonData = new
            {
                draw = draw,
                recordsFiltered = totalRecords,
                recordsTotal = totalRecords,
                data = userData.Select((u, i) => new {
                    u.UserName,
                    u.Email,
                    Roles = roles[i],
                    u.Id
                }).ToList()
            };

            return Ok(jsonData);
        }



        [HttpGet]
        public IActionResult EditUser(string userId)
        {
            // Kullanıcıyı bul
            var cUser = _context.Users.Find(userId);

            // Kullanıcı bulunamazsa hata dön
            if (cUser == null)
            {
                ModelState.AddModelError("404", "Kullanıcı Bulunamadı");
            }
            EditUserModel user = new EditUserModel
            {
                Id = cUser.Id,
                DateOfBirth = cUser.DateOfBirth,
                Email = cUser.Email,
                FullName = cUser.FullName,
                Gender = cUser.Gender
            };
            user.Role = _userManager.GetRolesAsync(cUser).Result.FirstOrDefault();
            var allRoles = _context.Roles.Select(s => new SelectListItem
            {
                Selected = s.Name.Equals(user.Role),
                Text = s.Name,
                Value = s.Name
            }).ToList();

            return View(new EditUserPageModel { User = user, AllRoles = allRoles });
        }

        [HttpPost]
        public async Task<IActionResult> EditUserAsync(EditUserModel user)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("402", "Kayıt İletilemedi");
                //new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }
                return View();
            }

            var cUser = _context.Users.Find(user.Id);

            if (cUser == null)
            {
                ModelState.AddModelError("404", "Kullanıcı Bulunamadı");
                //new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier }
                return View();
            }

            cUser.FullName = user.FullName;
            cUser.Email = user.Email;
            cUser.Gender = user.Gender;
            cUser.DateOfBirth = user.DateOfBirth;
            _context.Update(cUser);
            await _context.SaveChangesAsync();

            // Yeni rolü ayarla
            // Kullanıcının rollerini değiştir
            var userOldRoles = _userManager.GetRolesAsync(cUser).Result;
            foreach (var oldRole in userOldRoles)
            {
                await _userManager.RemoveFromRoleAsync(cUser, oldRole);
            }

            var result = await _userManager.AddToRoleAsync(cUser, user.Role);
            if (result.Succeeded)
            {
                return RedirectToAction("EditUser", new { userId = cUser.Id });
                // Değişikliği veritabanına kaydet
                // Başarılı bir şekilde rol değiştirildi
            }
            else
            {
                ModelState.AddModelError("404", "Rol değiştirilemedi.");
                return View("Error");
            }


        }
        //Kullanıcıyı silme
        [HttpGet]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost, ActionName("DeleteUser")]

        public async Task<IActionResult> DeleteUserConfirmed(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                // Silme işlemi başarısız olursa yapılacak işlemler
                return View("Error");
            }

            return RedirectToAction("Index", "Admin");
        }




    }
}
