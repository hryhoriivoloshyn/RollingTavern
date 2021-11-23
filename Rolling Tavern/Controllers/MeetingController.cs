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
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Rolling_Tavern.Data;
using Rolling_Tavern.Models;

namespace Rolling_Tavern.Controllers
{
    public class MeetingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private IWebHostEnvironment _appEnvironment;

        public MeetingController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment appEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _appEnvironment = appEnvironment;
        }
        public class CurrentInfo
        {
            public ApplicationUser CurrentUser;
            public Meeting CurrentMeeting;
            public CurrentInfo() {}
            public CurrentInfo(ApplicationUser user ,Meeting meeting)
            {
                CurrentUser = user;
                CurrentMeeting = meeting;
            }
        }
        private async Task<string> UploadPicture(IFormFile profilePicture, Meeting meeting)
        {
            const string defaultPicturePath = "/MeetingPictures/DefaultUser.png";
            if (profilePicture == null)
            {
                return defaultPicturePath;
            }
            var user = await _userManager.GetUserAsync(User);
            string userId = await _userManager.GetUserIdAsync(user);
            string format = "Mddyyyyhhmmsstt";
            string imagename = String.Format("{0}", DateTime.Now.ToString(format));
            string pictureType = profilePicture.ContentType;
            string pictureExtension = pictureType.Substring(pictureType.IndexOf("/") + 1);
            string profilePicturePath = "/MeetingPictures/" + userId + meeting.MeetingName[0] + imagename + "." + pictureExtension;

            using (var fileStream = new FileStream(_appEnvironment.WebRootPath + profilePicturePath, FileMode.Create))
            {
                await profilePicture.CopyToAsync(fileStream);
            }

            return profilePicturePath;
        }

        private async Task<List<Meeting>> GetMeetings()
        {
            List<Meeting> Meetings = new List<Meeting>();
            var allMeetings = await _context.Meetings.Where(d => d.DateOfMeeting > DateTime.Now && d.Creator != null).ToListAsync();
            if(allMeetings?.Any()==true)
            {
                foreach(var item in allMeetings)
                {
                    List<Request> requests = await _context.Requests.Where(i => i.MeetingId == item.MeetingId).ToListAsync();
                    ApplicationUser creator = await _context.Users.FirstOrDefaultAsync(u => u.Id == item.CreatorId);
                    BoardGame game = await _context.BoardGames.Where(g => g.GameId == item.GameId).FirstOrDefaultAsync();
                    Meetings.Add(new Meeting
                    {
                        MeetingId = item.MeetingId,
                        MeetingName = item.MeetingName,
                        DateOfMeeting = item.DateOfMeeting,
                        AddresOfMeeting = item.AddresOfMeeting,
                        Description = item.Description,
                        AdditionalRequirements = item.AdditionalRequirements,
                        PhotoLink = item.PhotoLink,
                        CreatorId = item.CreatorId,
                        MinimalAge = item.MinimalAge,
                        GameId = item.GameId,
                        Game = game,
                        Creator = creator,
                        Requests = requests
                    });
                }    
            }
            return Meetings;
        }

        // GET: Meeting
        public async Task<ViewResult> Index()
        {
            var data = await GetMeetings();
            return View(data);
        }

        // GET: Meeting/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return null;
            }
            else
            {
                ApplicationUser currentUser = await _userManager.GetUserAsync(User);
                var temp = await _context.Meetings.Where(m => m.MeetingId == id).FirstOrDefaultAsync();
                BoardGame game = await _context.BoardGames.Where(i => i.GameId == temp.GameId).FirstOrDefaultAsync();
                ApplicationUser creator = await _context.Users.Where(i => i.Id == temp.CreatorId).FirstOrDefaultAsync();
                List<Request> requests = await _context.Requests.Where(i => i.MeetingId == temp.MeetingId).ToListAsync();
                Meeting meeting = new Meeting
                {
                    MeetingId = temp.MeetingId,
                    MeetingName = temp.MeetingName,
                    DateOfMeeting = temp.DateOfMeeting,
                    AddresOfMeeting = temp.AddresOfMeeting,
                    Description = temp.Description,
                    AdditionalRequirements = temp.AdditionalRequirements,
                    PhotoLink = temp.PhotoLink,
                    CreatorId = temp.CreatorId,
                    MinimalAge = temp.MinimalAge,
                    GameId = temp.GameId,
                    Game = game,
                    Creator = creator,
                    Requests = requests
                };
                if (meeting == null)
                {
                    return null;
                }
                CurrentInfo info = new()
                {
                    CurrentUser = currentUser,
                    CurrentMeeting = meeting
                };
                return View(info);
            }
        }

        // GET: Meeting/Create
        [HttpGet]
        [Authorize]
        public IActionResult Create()
        {
            ViewData["GameId"] = new SelectList(_context.BoardGames, "GameId", "GameName");
            return View();
        }

        // POST: Meeting/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MeetingId,MeetingName,DateOfMeeting,AddresOfMeeting,Description,AdditionalRequirements,PhotoLink,CreatorId,GameId,MinimalAge")] Meeting meeting, IFormFile meetingPicture)
        {
            if (ModelState.IsValid)
            {
                var picturePath = await UploadPicture(meetingPicture, meeting);
                var loginedUser = await _userManager.GetUserAsync(User);
                long userId = Convert.ToInt64(await _userManager.GetUserIdAsync(loginedUser));
                Meeting createdMeeting = new Meeting
                {
                    MeetingName = meeting.MeetingName,
                    DateOfMeeting = meeting.DateOfMeeting,
                    AddresOfMeeting = meeting.AddresOfMeeting,
                    Description = meeting.Description,
                    AdditionalRequirements = meeting.AdditionalRequirements,
                    PhotoLink = picturePath,
                    CreatorId = userId,
                    GameId = meeting.GameId,
                    MinimalAge = meeting.MinimalAge
                };
                _context.Add(createdMeeting);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CreatorId"] = new SelectList(_context.Users, "Id", "Id", meeting.CreatorId);
            ViewData["GameId"] = new SelectList(_context.BoardGames, "GameId", "GameName", meeting.GameId);
            return View(meeting);
        }

        // GET: Meeting/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            else
            {
                var temp = await _context.Meetings.Where(m => m.MeetingId == id).FirstOrDefaultAsync();
                BoardGame game = await _context.BoardGames.Where(i => i.GameId == temp.GameId).FirstOrDefaultAsync();
                ApplicationUser creator = await _context.Users.Where(i => i.Id == temp.CreatorId).FirstOrDefaultAsync();
                List<Request> requests = await _context.Requests.Where(i => i.MeetingId == temp.MeetingId).ToListAsync();
                Meeting meeting = new Meeting
                {
                    MeetingId = temp.MeetingId,
                    MeetingName = temp.MeetingName,
                    DateOfMeeting = temp.DateOfMeeting,
                    AddresOfMeeting = temp.AddresOfMeeting,
                    Description = temp.Description,
                    AdditionalRequirements = temp.AdditionalRequirements,
                    PhotoLink = temp.PhotoLink,
                    CreatorId = temp.CreatorId,
                    MinimalAge = temp.MinimalAge,
                    GameId = temp.GameId,
                    Game = game,
                    Creator = creator,
                    Requests = requests
                };
                if (meeting == null)
                {
                    return null;
                }
                if (meeting == null)
                {
                    return NotFound();
                }
                ViewData["GameId"] = new SelectList(_context.BoardGames, "GameId", "GameName", meeting.GameId);
                return View(meeting);
            }
        }

        // POST: Meeting/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MeetingId,MeetingName,DateOfMeeting,AddresOfMeeting,Description,AdditionalRequirements,PhotoLink,CreatorId,GameId,MinimalAge")] Meeting meeting, IFormFile meetingPicture)
        {
/*            if (id != meeting.MeetingId)
            {
                return NotFound();
            }*/

            if (ModelState.IsValid)
            {
                try
                {
                    var picturePath = "new";
                    if (meetingPicture==null)
                    {
                        var meet = await _context.Meetings.Where(i=>i.MeetingId==id).FirstOrDefaultAsync();
                        picturePath = meet.PhotoLink;
                    }
                    else
                    {
                        picturePath = await UploadPicture(meetingPicture, meeting);
                    }
                    var loginedUser = await _userManager.GetUserAsync(User);
                    long userId = Convert.ToInt64(await _userManager.GetUserIdAsync(loginedUser));
                    meeting.MeetingId = id;
                    meeting.PhotoLink = picturePath;
                    meeting.CreatorId = userId;
                    Meeting meetingChanged = await _context.Meetings.Where(i => i.MeetingId == meeting.MeetingId).FirstAsync();
                    meetingChanged.MeetingId = meeting.MeetingId;
                    meetingChanged.MeetingName = meeting.MeetingName;
                    meetingChanged.DateOfMeeting = meeting.DateOfMeeting;
                    meetingChanged.AddresOfMeeting = meeting.AddresOfMeeting;
                    meetingChanged.Description = meeting.Description;
                    meetingChanged.AdditionalRequirements = meeting.AdditionalRequirements;
                    meetingChanged.PhotoLink = meeting.PhotoLink;
                    meetingChanged.CreatorId = meeting.CreatorId;
                    meetingChanged.GameId = meeting.GameId;
                    meetingChanged.MinimalAge = meeting.MinimalAge;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MeetingExists(meeting.MeetingId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CreatorId"] = new SelectList(_context.Users, "Id", "Id", meeting.CreatorId);
            ViewData["GameId"] = new SelectList(_context.BoardGames, "GameId", "GameName", meeting.GameId);
            return View(meeting);
        }

        // GET: Meeting/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var meeting = await _context.Meetings
                .Include(m => m.Creator)
                .Include(m => m.Game)
                .FirstOrDefaultAsync(m => m.MeetingId == id);
            if (meeting == null)
            {
                return NotFound();
            }

            return View(meeting);
        }

        // POST: Meeting/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var meeting = await _context.Meetings.FindAsync(id);
            _context.Meetings.Remove(meeting);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MeetingExists(int id)
        {
            return _context.Meetings.Any(e => e.MeetingId == id);
        }
    }
}
