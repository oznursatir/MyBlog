using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyBlog.Data;
using MyBlog.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using X.PagedList;
using MyBlog.Data.Migrations;
using System.Xml.Linq;
using System.Drawing.Printing;

namespace MyBlog.Controllers
{


    [Authorize(Roles ="Admin")]
    public class AdminController : Controller
    {


        //Veritabanı Modeline Erişim

        private readonly ApplicationDbContext _context;
        private readonly UserManager<CustomUser> _userManager;

        public AdminController(ApplicationDbContext context,UserManager<CustomUser> userManager)
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

        [HttpPost]
        public async Task<IActionResult> LoadPosts()
        {
            var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();

            var posts = _context.BlogPosts.AsQueryable();

            if (!string.IsNullOrEmpty(searchValue))
            {
                var normalizedSearch = searchValue.ToLower().Replace(" ", "");
                posts = posts.Where(p => p.Title.ToLower().Contains(normalizedSearch) ||
                                         p.Author.ToLower().Contains(normalizedSearch));
            }

            var totalRecords = await posts.CountAsync();
            var postData = await posts.Skip(int.Parse(start)).Take(int.Parse(length)).ToListAsync();

            var jsonData = new
            {
                draw = draw,
                recordsFiltered = totalRecords,
                recordsTotal = totalRecords,
                data = postData.Select(p => new {
                    p.Title,
                    p.Author,
                    CreatedAt = p.CreatedAt.ToString("g"),
                    p.Id
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
            var allRoles = _context.Roles.Select(s=> new SelectListItem
            {
                Selected = s.Name.Equals(user.Role),
                Text = s.Name,
                Value = s.Name
            }).ToList();

            return View(new EditUserPageModel {User = user,AllRoles=allRoles });
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
            cUser.Email= user.Email;
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
                return RedirectToAction("EditUser", new {userId=cUser.Id});
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

            return RedirectToAction("Index");
        }


        //Create(Oluşturma) İşlemi

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(BlogPost blogPost)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    blogPost.Author = user.FullName;
                }

                // Model verilerini veritabanına kaydetme işlemi
                blogPost.CreatedAt = DateTime.Now;
                _context.BlogPosts.Add(blogPost);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(blogPost);
        }

        //Update(Güncelleme) İşlemi

        [HttpGet]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null) return NotFound();

            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost == null) return NotFound();

            return View(blogPost);
        }

        [HttpPost]
        public async Task<IActionResult> EditPost(int id, BlogPost blogPost)
        {
            if (id != blogPost.Id) return NotFound();


            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user != null)
                    {
                        blogPost.Author = user.FullName;
                    }
                    blogPost.CreatedAt = DateTime.Now;
                    _context.Update(blogPost);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlogPostExists(blogPost.Id)) return NotFound();
                    else throw;
                }
            }
            return View(blogPost);
        }

        private bool BlogPostExists(int id)
        {
            return _context.BlogPosts.Any(e => e.Id == id);
        }

        // Delete (Silme) İşlemi

        [HttpGet]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _context.BlogPosts.FindAsync(id);
            if (post == null) return NotFound();

            return View(post);
        }


        [HttpPost, ActionName("DeletePost")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePostConfirmed(int id)
        {
            var post = await _context.BlogPosts.FindAsync(id);
            _context.BlogPosts.Remove(post);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        //İçerik görüntüleme (detay)
        [HttpGet]
        public async Task<IActionResult> Details(int? id, int page = 1, int pageSize = 5)
        {
            if (id == null)
            {
                return NotFound();
            }
            var blogPosts = await _context.BlogPosts
                                            .Include(bp => bp.Comments)
                                            .ThenInclude(c => c.User)
                                            .FirstOrDefaultAsync(bp=>bp.Id == id);
            if (blogPosts == null)
            {
                return NotFound();
            }
            var comments = blogPosts.Comments.AsQueryable().ToPagedList(page, pageSize);
            var viewModel = new BlogPostCommentsViewModel
            {
                BlogPost = blogPosts,
                PagedComments = comments
            };

            return View(viewModel);
        }

       

    }

}
