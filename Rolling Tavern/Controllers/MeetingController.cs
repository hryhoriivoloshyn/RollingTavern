using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
                    ApplicationUser creator = await _userManager.GetUserAsync(User);
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

        // GET: Meeting/Create
        [HttpGet]
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

            var meeting = await _context.Meetings.FindAsync(id);
            if (meeting == null)
            {
                return NotFound();
            }
            ViewData["CreatorId"] = new SelectList(_context.Users, "Id", "Id", meeting.CreatorId);
            ViewData["GameId"] = new SelectList(_context.BoardGames, "GameId", "GameName", meeting.GameId);
            return View(meeting);
        }

        // POST: Meeting/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MeetingId,MeetingName,DateOfMeeting,AddresOfMeeting,Description,AdditionalRequirements,PhotoLink,CreatorId,GameId,MinimalAge")] Meeting meeting)
        {
            if (id != meeting.MeetingId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(meeting);
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
