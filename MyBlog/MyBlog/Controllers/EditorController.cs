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
    [Authorize(Roles = "Admin, Editor")]
    public class EditorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<CustomUser> _userManager;
        public EditorController(ApplicationDbContext context, UserManager<CustomUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        //Read(Okuma) İşlemi
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            var posts = await _context.BlogPosts
                .Where(post => post.Author == user.FullName)
                .ToListAsync();

            return View(posts);
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
        public async Task<IActionResult> Edit(int? id)
        {

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
                        blogPost.Author = user.FullName;
                    }

                    // CreatedAt alanını güncellemeyelim
                    _context.Entry(blogPost).Property(x => x.CreatedAt).IsModified = false;

                    _context.Update(blogPost);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
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

        // Delete (Silme) İşlemi
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
            return RedirectToAction("Index");
        }

        //İçerik görüntüleme (detay)
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var blogPosts= await _context.BlogPosts.FindAsync(id);
            if (blogPosts==null)
            { 
                return NotFound();
            }
            return View(blogPosts);
        }
        [HttpPost]
        public async Task<IActionResult> UploadImageAsync(IFormFile upload)
        {
            if (upload != null)
            {
                var extent = Path.GetExtension(upload.FileName);
                var randomName = ($"{Guid.NewGuid()}{extent}");
                var imagePath = "images\\post\\"+randomName;
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imagePath);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await upload.CopyToAsync(stream);
                }
                return Json(new { url = "/"+imagePath.Replace("\\","/") });
            }

            return BadRequest("Resim dosyası yüklenemedi.");
        }

    }
}


    

