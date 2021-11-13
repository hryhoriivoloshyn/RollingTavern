using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Rolling_Tavern.Models;

namespace Rolling_Tavern.Areas.Identity.Pages.Account.Manage
{
    public partial class _ManageNavModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private Data.ApplicationDbContext db;


        public _ManageNavModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            Data.ApplicationDbContext dbContext
           )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            db = dbContext; 
        }

        public ApplicationUser UserInfo { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            UserInfo= user;
            return Page();
        }

     

    }
}
