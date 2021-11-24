using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Rolling_Tavern.Models;

namespace Rolling_Tavern.Areas.Identity.Pages.Account.Manage
{
    [Authorize(Roles="user")]
    public partial class EditProfileModel : PageModel
    {
        private readonly IWebHostEnvironment _appEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private Data.ApplicationDbContext db;

        public EditProfileModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            Data.ApplicationDbContext dbContext,
            IWebHostEnvironment appEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            db = dbContext;
            _appEnvironment = appEnvironment;
        }

        public ApplicationUser UserInfo { get; set; }


        public string Username { get; set; }
        public string Email { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {



            [Required]
            [StringLength(32, ErrorMessage = "The {0} must be at max {1} characters long.")]
            [DataType(DataType.Text)]
            [Display(Name = "Ім'я*")]
           
            public string FirstName { get; set; }

            [Required]
            [StringLength(32, ErrorMessage = "The {0} must be at max {1} characters long.")]
            [DataType(DataType.Text)]
            [Display(Name = "Призвище*")]
            public string LastName { get; set; }


            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [DataType(DataType.Upload)]
            [Display(Name = "Завантажте фото профілю")]
            [AllowedExtensions(new string[] { ".png", ".jpg", "jpeg", ".gif" })]
            public IFormFile ProfilePicture { get; set; }
        }

   
        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;
            Email = email;
            UserInfo = user;


            Input = new InputModel
            {
               
                PhoneNumber = phoneNumber,
                FirstName  = user.FirstName,
                LastName = user.LastName
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        private async Task UploadPicture(IFormFile profilePicture, ApplicationUser user)
        {
            if (profilePicture != null)
            {
                string pictureType = profilePicture.ContentType;
                string pictureExtension = pictureType.Substring(pictureType.IndexOf("/") + 1);
                var email = await _userManager.GetEmailAsync(user);
                string profilePicturePath = "/ProfilePictures/" + email + "." + pictureExtension;

                if (user.ProfilePicture != GlobalVariables.DefaultUserImage)
                {
                  
                    FileInfo oldPicture = new FileInfo(_appEnvironment.WebRootPath + user.ProfilePicture);
                    if (oldPicture.Exists)
                    {
                        oldPicture.Delete();
                    }
                    
                }

                using (var fileStream =
                        new FileStream(_appEnvironment.WebRootPath + profilePicturePath, FileMode.Create))
                    {
                        
                        await profilePicture.CopyToAsync(fileStream);
                    }

                    user.ProfilePicture = profilePicturePath;

                
               
            }
        }


        public async Task<IActionResult> OnPostAsync()
            {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }
            
            await UploadPicture(Input.ProfilePicture, user);
            user.FirstName = Input.FirstName;
            user.LastName = Input.LastName;
            user.PhoneNumber = Input.PhoneNumber;
            db.Users.Update(user);
            await db.SaveChangesAsync();
            return RedirectToPage();


            //По идее так писать профессиональнее, но для кастомных полей сделать логику сложнее
            //var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            //if (Input.PhoneNumber != phoneNumber)
            //{
            //    var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
            //    if (!setPhoneResult.Succeeded)
            //    {
            //        StatusMessage = "Unexpected error when trying to set phone number.";
            //        return RedirectToPage();
            //    }
            //}





            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
