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

         }

}
