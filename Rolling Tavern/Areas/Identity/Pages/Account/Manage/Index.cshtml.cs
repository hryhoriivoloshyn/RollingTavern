using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Rolling_Tavern.Controllers;
using Rolling_Tavern.Models;

namespace Rolling_Tavern.Areas.Identity.Pages.Account.Manage
{
    [Authorize(Roles="user")]
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private Data.ApplicationDbContext db;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            Data.ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            db = dbContext;
        }

        public ApplicationUser UserInfo { get; set; }

        public IEnumerable<Meeting> CreatedMeetings { get; set; }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }
        }

        private async Task<List<Meeting>> GetMeetingCreatorAsync(ApplicationUser user)
        {
            long userId = Convert.ToInt64(await _userManager.GetUserIdAsync(user));
            List<Meeting> data = new List<Meeting>();
            List<Meeting> tempData = await db.Meetings.Where(m => m.CreatorId == userId).ToListAsync();
            if(tempData?.Any()==true)
            {
                foreach(var item in tempData)
                {
                    BoardGame game = db.BoardGames.Where(m => m.GameId == item.GameId).First();
                    List<Request> requests = await db.Requests.Where(r => r.MeetingId == item.MeetingId).ToListAsync();
                    data.Add(new Meeting()
                    {
                        MeetingId = item.MeetingId,
                        MeetingName = item.MeetingName,
                        DateOfMeeting = item.DateOfMeeting,
                        AddresOfMeeting = item.AddresOfMeeting,
                        Description = item.Description,
                        AdditionalRequirements = item.AdditionalRequirements,
                        PhotoLink = item.PhotoLink,
                        Creator = user,
                        CreatorId = userId,
                        Game = game,
                        GameId = item.GameId,
                        Requests = requests
                    });
                }
            }
            return data;
        }

        private async Task<List<Meeting>> GetMeetingsAsync(ApplicationUser user)
        {
            long userId = Convert.ToInt64(await _userManager.GetUserIdAsync(user));
            var role = await _userManager.GetRolesAsync(user);
            List<Request> meetingsId = await db.Requests.Where(r => r.UserId == userId && r.StateId == 2).ToListAsync();
            List<Meeting> data = new List<Meeting>();
            if(meetingsId?.Any()==true)
            {
                foreach (var i in meetingsId)
                {
                    Meeting meeting = db.Meetings.Where(m => m.MeetingId == i.MeetingId).First();
                    ApplicationUser Creator = new ApplicationUser();
                    long? CreatorId = 0;
                    if(meeting.CreatorId==null)
                    {
                        Creator = null;
                        CreatorId = null;
                    }
                    else
                    {
                        CreatorId = meeting.CreatorId;
                        Creator = await db.Users.Where(u => u.Id == CreatorId).FirstOrDefaultAsync();
                    }
                    BoardGame game = db.BoardGames.Where(m => m.GameId == meeting.GameId).First();
                    List<Request> requests = await db.Requests.Where(r => r.MeetingId == meeting.GameId).ToListAsync();
                    data.Add(new Meeting()
                    {
                        MeetingId = meeting.MeetingId,
                        MeetingName = meeting.MeetingName,
                        DateOfMeeting = meeting.DateOfMeeting,
                        AddresOfMeeting = meeting.AddresOfMeeting,
                        Description = meeting.Description,
                        AdditionalRequirements = meeting.AdditionalRequirements,
                        PhotoLink = meeting.PhotoLink,
                        Creator = Creator,
                        CreatorId = CreatorId,
                        MinimalAge = meeting.MinimalAge,
                        Game = game,
                        GameId = meeting.GameId,
                        Requests = requests
                    });
                }
            }
            return data;
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

            Username = userName;

            UserInfo = user;
            var createdMeetings = await GetMeetingCreatorAsync(user);
            var appliedMeetings = await GetMeetingsAsync(user);
            List<Meeting> data = new List<Meeting>();
            foreach(var item in createdMeetings)
            {
                data.Add(item);
            }
            foreach(var item in appliedMeetings)
            {
                data.Add(item);
            }
            CreatedMeetings = data.OrderByDescending(d => d.DateOfMeeting);

            Input = new InputModel
            {
                PhoneNumber = phoneNumber
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

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
