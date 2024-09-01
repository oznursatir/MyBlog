using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBlog.Data;
using MyBlog.Models;
using System.Linq;


namespace MyBlog.Controllers
{
    public class BlogPostController : Controller
    {
        //Veritabanı Modeline Erişim

        private readonly ApplicationDbContext _context;
        private readonly UserManager<CustomUser> _userManager;

        public BlogPostController(ApplicationDbContext context, UserManager<CustomUser> userManager)

        {
            _context = context;
            _userManager = userManager;
        }



        [Authorize(Roles = "Admin, Editor, User")]
        public async Task<IActionResult> Index()
        {
            var contents = await _context.BlogPosts.ToListAsync();

            foreach (var content in contents)
            {
                content.User = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == content.UserId);
            }

            return View(contents);

            
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            var model = new List<BlogPost>();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                model = await _context.BlogPosts
                    .Where(b => b.Title.Contains(searchTerm) || b.UserId.Contains(searchTerm))
                    .ToListAsync();
            }
            else
            {
                model = await _context.BlogPosts.ToListAsync();
            }

            return View("Index", model);
        }

        [Authorize(Roles = "Admin, Editor")]
        [HttpPost]
        public async Task<IActionResult> LoadPosts(bool isMode = false)
        {
            var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();

            var posts = _context.BlogPosts.AsQueryable();
            var user = await _userManager.GetUserAsync(User);

            if (isMode || User.IsInRole("Editor"))
            {
                posts = posts.Where(post => post.UserId == user.Id);
            }

            if (!string.IsNullOrEmpty(searchValue))
            {
                var normalizedSearch = searchValue.ToLower().Replace(" ", "");
                posts = posts.Where(p => p.Title.ToLower().Contains(normalizedSearch) ||
                                         p.UserId.ToLower().Contains(normalizedSearch));
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
                    p.UserId,
                    CreatedAt = p.CreatedAt.ToString("g"),
                    Author = _context.Users.Where(u => u.Id == p.UserId).Select(u => u.FullName).FirstOrDefault(), // UserId ile yazar adı çekiliyor
                    p.Id
                }).ToList()
            };

            return Ok(jsonData);
        }

        public async Task<IActionResult> Details(int id)
        {
            var content = await _context.BlogPosts.FirstOrDefaultAsync(b => b.Id == id);

            if (content == null)
            {
                return NotFound();
            }

            content.User = await _context.Users
        .FirstOrDefaultAsync(u => u.Id == content.UserId);

            return View(content);

        }




        //Read(Okuma) İşlemi
        [Authorize(Roles = "Admin, Editor")]
        public async Task<IActionResult> Read(bool isMode = true)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var posts = await _context.BlogPosts
                .Where(post => post.UserId == user.Id)
                .ToListAsync();

            return View(posts);
        }


        //Create(Oluşturma) İşlemi
        [Authorize(Roles = "Admin, Editor")]
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
                    blogPost.UserId = user.Id;
                }

                // Model verilerini veritabanına kaydetme işlemi
                blogPost.CreatedAt = DateTime.Now;
                _context.BlogPosts.Add(blogPost);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(blogPost);
        }

        [HttpPost]
        public async Task<IActionResult> UploadImageAsync(IFormFile upload)
        {
            if (upload != null)
            {
                var extent = Path.GetExtension(upload.FileName);
                var randomName = ($"{Guid.NewGuid()}{extent}");
                var imagePath = "images\\post\\" + randomName;
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imagePath);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await upload.CopyToAsync(stream);
                }
                return Json(new { url = "/" + imagePath.Replace("\\", "/") });
            }

            return BadRequest("Resim dosyası yüklenemedi.");
        }



        // Delete (Silme) İşlemi
        [Authorize(Roles = "Admin, Editor")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _context.BlogPosts.FindAsync(id);
            if (post == null) return NotFound();

            return View(post);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.BlogPosts.FindAsync(id);
            _context.BlogPosts.Remove(post);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Admin");
        }


        //Update(Güncelleme) İşlemi
        [Authorize(Roles = "Admin, Editor")]

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var blogPost = await _context.BlogPosts.FindAsync(id);
            if (blogPost == null)
                return NotFound();

            return View(blogPost);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, BlogPost blogPost)
        {
            if (id != blogPost.Id) return NotFound();


            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user != null)
                    {
                        blogPost.UserId = user.Id;
                    }

                    // CreatedAt alanını güncellemeyelim
                    _context.Entry(blogPost).Property(x => x.CreatedAt).IsModified = false;

                    _context.Update(blogPost);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Admin");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlogPostExists(blogPost.Id))
                        return NotFound();
                    else throw;
                }
            }
            return View(blogPost);
        }

        private bool BlogPostExists(int id)
        {
            return _context.BlogPosts.Any(e => e.Id == id);
        }
    }

}
