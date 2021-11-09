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

        public string MeetingNameModel { get; set; }
        public string PhotoLinkModel { get; set; }
        public DateTime DateOfMeetingModel { get; set; }
        public string AddresOfMeetingModel { get; set; }
        public string DescriptionModel { get; set; }
        public string AdditionalRequirementsModel { get; set; }
        public int GameIdModel { get; set; }
        public int CreatorIdModel { get; set; }

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
            string profilePicturePath = "/MeetingPictures/" + userId + meeting.MeetingName.Trim() + imagename + "." + pictureExtension;

            using (var fileStream = new FileStream(_appEnvironment.WebRootPath + profilePicturePath, FileMode.Create))
            {
                await profilePicture.CopyToAsync(fileStream);
            }

            return profilePicturePath;
        }

        // GET: Meeting
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Meetings.Include(m => m.Creator).Include(m => m.Game);
            return View(await applicationDbContext.ToListAsync());
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
        public async Task<IActionResult> Create([Bind("MeetingId,MeetingName,DateOfMeeting,AddresOfMeeting,Description,AdditionalRequirements,PhotoLink,CreatorId,GameId")] Meeting meeting, IFormFile meetingPicture)
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
                    GameId = meeting.GameId
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
        public async Task<IActionResult> Edit(int id, [Bind("MeetingId,MeetingName,DateOfMeeting,AddresOfMeeting,Description,AdditionalRequirements,PhotoLink,CreatorId,GameId")] Meeting meeting)
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
