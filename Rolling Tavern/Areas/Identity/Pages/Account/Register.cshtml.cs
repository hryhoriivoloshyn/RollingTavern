using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Rolling_Tavern.Models;

namespace Rolling_Tavern.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    [Authorize(Roles = "user")]
    public class RegisterModel : PageModel
    {
        private readonly IWebHostEnvironment _appEnvironment;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        
        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender, IWebHostEnvironment appEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _appEnvironment = appEnvironment;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Електронна пошта")]
            [RegularExpression(@"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*"
                + "@"
                + @"((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$",
                ErrorMessage = "Невірно набрана пошта")]
            public string Email { get; set; }

            [Required]
            [Display(Name="Нікнейм")]
            [StringLength(32, ErrorMessage = "{0} повинно бути не більше {1} літер")]
            public string UserName { get; set; }

            [Phone]
            [Display(Name="Номер телефону")]
            [DataType(DataType.PhoneNumber)]
            [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Введене значення не відповідає номеру телефона")]
            public string Phone { get; set; }

            [Required]
            [StringLength(32, ErrorMessage = "{0} повинно бути не більше {1} літер")]
            [DataType(DataType.Text)]
            [Display(Name = "Ім'я*")]
            public string FirstName { get; set; }


            [Required]
            [StringLength(32, ErrorMessage = "{0} повинен бути не більше {1} літер")]
            [DataType(DataType.Text)]
            [Display(Name = "Прізвище*")]
            public string LastName { get; set; }

            [Required]
            [DataType(DataType.Date)]
            [Display(Name = "Дата народження*")]
            public DateTime DateOfBirth { get; set; }

            [Required]
            [StringLength(24, ErrorMessage = "{0} повинен бути від {2} до {1} літер", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Пароль*")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Повторіть пароль*")]
            [Compare("Password", ErrorMessage = "Паролі не співпадають.")]
            public string ConfirmPassword { get; set; }

            [DataType(DataType.Upload)]
            [AllowedExtensions(new string[]{".png",".jpg","jpeg",".gif"})]
            [Display(Name ="Завантажте фото профілю")]
            public IFormFile  ProfilePicture { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        private async Task<string> UploadPicture(IFormFile profilePicture)
        {   
            if (profilePicture == null)
            {
                return null;
            }

            string pictureType = profilePicture.ContentType;
            string pictureExtension = pictureType.Substring(pictureType.IndexOf("/") + 1);
            string profilePicturePath = "/ProfilePictures/" + Input.Email + "." + pictureExtension;

            using (var fileStream = new FileStream(_appEnvironment.WebRootPath + profilePicturePath, FileMode.Create))
            {
                await profilePicture.CopyToAsync(fileStream);
            }

            return profilePicturePath;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            

            if (ModelState.IsValid)
            {
                var userWithSameMail = await _userManager.FindByEmailAsync(Input.Email);
                if (userWithSameMail!=null)
                {
                    ModelState.AddModelError(string.Empty, "Така поштова скринька вже існує");
                    return Page();
                }
                var picturePath = await UploadPicture(Input.ProfilePicture);
                var user = new ApplicationUser { UserName = Input.UserName, PhoneNumber = Input.Phone, Email = Input.Email, FirstName = Input.FirstName, LastName = Input.LastName, DateOfBirth = Input.DateOfBirth, ProfilePicture = picturePath };
               
                var result = await _userManager.CreateAsync(user, Input.Password);
                
                if (result.Succeeded)
                {
                    var currentUser = await _userManager.FindByNameAsync(user.UserName);
                    var roleResult = await _userManager.AddToRoleAsync(currentUser, "user");

                    _logger.LogInformation("User created a new account with password.");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
