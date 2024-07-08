using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public async Task<IActionResult> Details(int id, int? page)
        {
            var content = await _context.BlogPosts
                .Include(b => b.Comments)
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (content == null)
            {
                return NotFound();
            }

            int pageSize = 5;
            int pageNumber = page ?? 1;

            var comments = content.Comments.OrderByDescending(c => c.CreatedAt).ToPagedList(pageNumber, pageSize);

            var viewModel = new BlogPostCommentsViewModel
            {
                BlogPost = content,
                PagedComments = comments
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_CommentsPartial", viewModel.PagedComments);
            }

            return View(viewModel);
        }

        [HttpPost]

        public async Task<IActionResult> AddComment(string content, int postId)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                // Hata mesajı gösterme, geri dönme veya başka bir işlem yapma
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var comment = new Models.Comment
            {
                Content = content,
                BlogPostId = postId,
                UserId = userId
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

        private bool CommentExists(int id)
        {
            return _context.Comments.Any(e => e.Id == id);
        }
    }
}
