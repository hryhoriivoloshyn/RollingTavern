using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Rolling_Tavern.Data;
using Rolling_Tavern.Models;

namespace Rolling_Tavern.Controllers
{
    [Authorize(Roles="admin")]
    public class AdminUserManageController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private IWebHostEnvironment _appEnvironment;

        public AdminUserManageController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment appEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _appEnvironment = appEnvironment;
        }

        [HttpGet]
        public async Task<ViewResult> Index()
        {
            var UserIds = _context.UserRoles.Where(r => r.RoleId == 1).Select(r=>r.UserId).ToArray();
            IEnumerable<ApplicationUser> users =  _context.Users.Where(u=>UserIds.Contains(u.Id)).ToList();
            
            return View(users);
        }
    }
}
