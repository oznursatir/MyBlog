using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
    [Authorize(Roles = "Admin, Editor, User")]
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
            var contents = await _context.BlogPosts.ToListAsync();
            return View(contents);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            var model = new List<BlogPost>();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                model = await _context.BlogPosts
                    .Where(b => b.Title.Contains(searchTerm) || b.Author.Contains(searchTerm))
                    .ToListAsync();
            }
            else
            {
                model = await _context.BlogPosts.ToListAsync();
            }

            return View("Index", model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var content = await _context.BlogPosts.FirstOrDefaultAsync(b => b.Id == id);

            if (content == null)
            {
                return NotFound();
            }

            var viewModel = new BlogPostCommentsViewModel
            {
                BlogPost = content,
            };

            return View(viewModel);
        }

        public async Task<IActionResult> GetComments(int blogId,int page)
        {
            var comments = _context.Comments.Where(w => w.BlogPostId == blogId).OrderByDescending(c => c.CreatedAt).Skip((page-1)*5).Take(5).Include(i=>i.User).ToPagedList();
            return PartialView("_CommentsPartial", comments);
        }

        [HttpPost]

        public async Task<IActionResult> AddComment(string content, int postId)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "Yorum içeriği boş olamaz." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var comment = new Models.Comment
            {
                Content = content,
                BlogPostId = postId,
                UserId = userId,
                CreatedAt = DateTime.Now
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = postId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteComment(int id, string returnUrl = null)
        {
            var comment = await _context.Comments.FindAsync(id);

            if (comment == null)
            {
                return NotFound();
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

            return RedirectToAction("Details", new { id = comment.BlogPostId }); // yorumun bağlı olduğu gönderiye geri dön
        }

        public IActionResult Privacy()
        {
            return View();
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
