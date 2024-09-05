using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using MyBlog.Data;
using MyBlog.Models;
using System.Diagnostics;
using System.Security.Claims;
using X.PagedList;

namespace MyBlog.Controllers
{
    [Authorize(Roles = "Admin, Editor, User")]
    public class BlogCommentsController : Controller
    {
        //Veritabanı Modeline Erişim

        private readonly ApplicationDbContext _context;
        private readonly UserManager<CustomUser> _userManager;

        public BlogCommentsController(ApplicationDbContext context, UserManager<CustomUser> userManager)

        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
            public async Task<IActionResult> Details(int postId)
        {
            var comments = await _context.Comments
                .Where(comment => comment.BlogPostId == postId)
                .Include(comment => comment.User)
                .ToListAsync();

            var blogPost = await _context.BlogPosts.FirstOrDefaultAsync(b => b.Id == postId);

            return PartialView("_CommentsPartial", comments);
        }

        public async Task<IActionResult> GetComments(int blogId, int page )
        {
            var comments = await _context.Comments
                .Where(c => c.BlogPostId == blogId)
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * 5)
                .Take(5)
                .ToListAsync();

            return PartialView("_CommentsPartial", comments); // PartialView ile HTML döndür
        }


        [HttpPost]

        public async Task<IActionResult> AddComment(string content, int postId)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "Yorum içeriği boş olamaz." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var addComment = new AddComment
            {
                Content = content,
                BlogPostId = postId,
                UserId = userId,
                CreatedAt = DateTime.Now
            };

            // AddComment modelini Comment modeline dönüştürüyoruz
            var comment = new Comment
            {
                Content = addComment.Content,
                BlogPostId = addComment.BlogPostId,
                UserId = addComment.UserId,
                CreatedAt = addComment.CreatedAt
                // İlişkiler buraya eklenmiyor, çünkü sadece ihtiyacımız olan alanları kullanıyoruz
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Yorum başarıyla eklendi.", addComment = comment });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteComment(int id, string returnUrl)
        {
            var comment = await _context.Comments.FindAsync(id);

            if (comment == null)
            {
                return Json(new { success = false, message = "Yorum bulunamadı." });
            }

            // Sadece mevcut kullanıcı kendi yorumunu silebilir
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");
            if (comment.UserId != currentUser.Id && !isAdmin)
            {
                return Forbid(); // 403 Forbidden döndür
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Details", "BlogPost", new { id = comment.BlogPostId }); // yorumun bağlı olduğu gönderiye geri dön
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public async Task<IActionResult> MyComments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var comments = await _context.Comments
                .Include(c => c.BlogPost)
                .Include(c => c.User)
                .Where(c => c.UserId == userId)
                .ToListAsync();


            return View(comments);
        }


        [HttpGet]
        public async Task<IActionResult> EditComment(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");
            if (comment.UserId != currentUser.Id && !isAdmin)
            {
                return Forbid();
            }

            var editComment = new EditComment
            {
                Id = comment.Id,
                CommentId = comment.Id,
                Text = comment.Content,

            };

            return View(editComment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditComment(int id, EditComment editComment)
        {
            if (id != editComment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var comment = await _context.Comments.FindAsync(editComment.CommentId);
                    if (comment == null)
                    {
                        return NotFound();
                    }

                    comment.Content = editComment.Text;

                    _context.Update(comment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentExists(editComment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(MyComments));
            }
            return View(editComment);
        }

        [HttpGet]
        public async Task<IActionResult> EditCommentDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin");
            if (comment.UserId != currentUser.Id && !isAdmin)
            {
                return Forbid();
            }

            var editComment = new EditComment
            {
                Id = comment.Id,
                CommentId = comment.Id,
                Text = comment.Content,

            };
            return PartialView(editComment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCommentDetails(int id, EditComment editComment)
        {
            if (id != editComment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var comment = await _context.Comments.FindAsync(editComment.CommentId);
                    if (comment == null)
                    {
                        return NotFound();
                    }

                    comment.Content = editComment.Text;

                    _context.Update(comment);
                    await _context.SaveChangesAsync();

                    return Json(new { success = true, postId = comment.BlogPostId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentExists(editComment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

            }

            return PartialView(editComment);
        }

        private bool CommentExists(int id)
        {
            return _context.Comments.Any(e => e.Id == id);
        }
    }
}
