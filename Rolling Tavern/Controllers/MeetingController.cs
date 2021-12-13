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
            public bool Role { get; set; }
            public CurrentInfo() {}
            public CurrentInfo(ApplicationUser user ,Meeting meeting)
            {
                CurrentUser = user;
                CurrentMeeting = meeting;
            }
        }
        public class UserInfo
        {
            public ApplicationUser User;
            public List<Meeting> Meetings;
            public UserInfo() {}
            public UserInfo(ApplicationUser user, List<Meeting> meetings)
            {
                User = user;
                Meetings = meetings;
            }
        }
        private async Task<string> UploadPicture(IFormFile profilePicture, Meeting meeting)
        {
            var game = await _context.BoardGames.Where(i => i.GameId == meeting.GameId).FirstOrDefaultAsync();
            string defaultPicturePath;
            if (game.GameId==1)
            {
                defaultPicturePath = "/GamePictures/defaultBoardGame.jpg";
            }
            else
            {
                var picrutures = await _context.GameImages.Where(i => i.GameId == meeting.GameId).ToListAsync();
                defaultPicturePath = picrutures.First().ImagePath;
            }
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


        private async Task<IEnumerable<Meeting>> GetMeetings()
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
        //public async Task<ViewResult> Index()
        //{
        //    var data = await GetMeetings();
        //    return View(data);
        //}


        public async Task<ViewResult> Index(string sortOrder, string searchString)
        {
            var meetings = await GetMeetings();
          
                ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            var games = await _context.BoardGames.ToListAsync();
            var users = await _context.Users.ToListAsync();
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["CurrentFilter"] = searchString;


            if (!String.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                meetings = meetings.Where(s => s.MeetingName.ToLower().Contains(searchString)
                                               || s.AddresOfMeeting.ToLower().Contains(searchString)
                                               || s.Game.GameName.ToLower().Contains(searchString)
                                               || s.Game.Genre.ToLower().Contains(searchString)
                                               || s.Creator.UserName.ToLower().Contains(searchString)
                                               || s.Creator.FirstName.ToLower().Contains(searchString)
                                               || s.Creator.LastName.ToLower().Contains(searchString));
            }

            switch (sortOrder)
            {
                case "name_desc":
                    meetings = meetings.OrderByDescending(s => s.MeetingName);
                    break;
                case "Date":
                    meetings = meetings.OrderBy(s => s.DateOfMeeting);
                    break;
                case "date_desc":
                    meetings = meetings.OrderByDescending(s => s.DateOfMeeting);
                    break;
                default:
                    meetings = meetings.OrderBy(s => s.MeetingName);
                    break;
            }
            return View(meetings);
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
                foreach(var item in requests)
                {
                    item.User = await _context.Users.Where(i => i.Id == item.UserId).FirstAsync();
                }
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "user")]
        public async Task<IActionResult> Details(int? id, Meeting meeting)
        {
            meeting = await _context.Meetings.Where(i => i.MeetingId == id).FirstOrDefaultAsync();
            ApplicationUser currentUser = await _userManager.GetUserAsync(User);
            Request request = new()
            {
                MeetingId = meeting.MeetingId,
                UserId = currentUser.Id,
                StateId = 1
            };
            _context.Add(request);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "user")]
        public async Task<IActionResult> ExitMeeting(int? meetingId)
        {
            Meeting meeting = await _context.Meetings.Where(i => i.MeetingId == meetingId).FirstOrDefaultAsync();
            ApplicationUser currentUser = await _userManager.GetUserAsync(User);
            Request request = await _context.Requests.Where(i => i.MeetingId == meeting.MeetingId && i.UserId == currentUser.Id).FirstOrDefaultAsync();
            if (DateTime.Now > meeting.DateOfMeeting.AddDays(-2))
            {
                currentUser.Rating -= 200;
                if(currentUser.Rating<0)
                {
                    currentUser.Rating = 0;
                }
                await _userManager.UpdateAsync(currentUser);
            }
            request.StateId = 4;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Meeting/Create
        [HttpGet]
        [Authorize(Roles = "user")]
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
        [Authorize(Roles = "user")]
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
        [Authorize(Roles = "user")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
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
                    return NotFound();
                }
                if (currentUser.Id != meeting.CreatorId)
                    return NotFound();
                ViewData["GameId"] = new SelectList(_context.BoardGames, "GameId", "GameName", meeting.GameId);
                return View(meeting);
            }
        }

        // POST: Meeting/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "user")]
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
        [Authorize(Roles = "user,admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ApplicationUser currentUser = await _userManager.GetUserAsync(User);
            bool role = await _userManager.IsInRoleAsync(currentUser, "admin");
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
                return NotFound();
            }
            if(!User.IsInRole("admin"))
            {
                if (meeting.CreatorId != currentUser.Id)
                    return NotFound();
            }
            CurrentInfo info = new()
            {
                CurrentUser = currentUser,
                CurrentMeeting = meeting
            };
            info.Role = role;
            return View(info);
        }

        // POST: Meeting/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "user,admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var meeting = await _context.Meetings.FindAsync(id);
            if(_context.Requests.Where(i => i.MeetingId == meeting.MeetingId).Any())
            {
                List<Request> requests = await _context.Requests.Where(i => i.MeetingId == meeting.MeetingId).ToListAsync();
                foreach (var item in requests)
                {
                    _context.Requests.Remove(item);
                }
            }
            _context.Meetings.Remove(meeting);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "user")]
        public async Task<IActionResult> ShowRequests(int? id)
        {
            List<Request> tempRequests = await _context.Requests.Where(i => i.MeetingId == id).ToListAsync();
            Meeting meeting = await _context.Meetings.Where(i => i.MeetingId == id).FirstOrDefaultAsync();
            List<Request> requests = new List<Request>();
            foreach(var item in tempRequests)
            {
                var user = await _context.Users.Where(i => i.Id == item.UserId).FirstOrDefaultAsync();
                var state = await _context.States.Where(i => i.StateId == item.StateId).FirstOrDefaultAsync();
                requests.Add(new()
                {
                    UserId = item.UserId,
                    MeetingId = item.MeetingId,
                    StateId = item.StateId,
                    User = user,
                    State = state
                });
            }
            meeting.Requests = requests;
            return View(meeting);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "user")]
        public async Task<IActionResult> ShowRequests(int id, int userId, string response)
        {
            if(response=="Додати")
            {
                Request request = await _context.Requests.Where(i => i.MeetingId == id && i.UserId == userId).FirstOrDefaultAsync();
                request.StateId = 2;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ShowRequests));
            }
            else if(response=="Видалити")
            {
                Request request = await _context.Requests.Where(i => i.MeetingId == id && i.UserId == userId).FirstOrDefaultAsync();
                request.StateId = 3;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ShowRequests));
            }
            else if(response=="Відмовити")
            {
                Request request = await _context.Requests.Where(i => i.MeetingId == id && i.UserId == userId).FirstOrDefaultAsync();
                request.StateId = 3;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ShowRequests));
            }
            return RedirectToAction(nameof(Details));
        }

        private async Task<List<Meeting>> GetMeetingCreatorAsync(ApplicationUser user)
        {
            long userId = Convert.ToInt64(await _userManager.GetUserIdAsync(user));
            List<Meeting> data = new List<Meeting>();
            List<Meeting> tempData = await _context.Meetings.Where(m => m.CreatorId == userId).ToListAsync();
            if (tempData?.Any() == true)
            {
                foreach (var item in tempData)
                {
                    BoardGame game = _context.BoardGames.Where(m => m.GameId == item.GameId).First();
                    List<Request> requests = await _context.Requests.Where(r => r.MeetingId == item.MeetingId).ToListAsync();
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
            List<Request> meetingsId = await _context.Requests.Where(r => r.UserId == userId && r.StateId == 2).ToListAsync();
            List<Meeting> data = new List<Meeting>();
            if (meetingsId?.Any() == true)
            {
                foreach (var i in meetingsId)
                {
                    Meeting meeting = _context.Meetings.Where(m => m.MeetingId == i.MeetingId).First();
                    ApplicationUser Creator = new ApplicationUser();
                    long? CreatorId = 0;
                    if (meeting.CreatorId == null)
                    {
                        Creator = null;
                        CreatorId = null;
                    }
                    else
                    {
                        CreatorId = meeting.CreatorId;
                        Creator = await _context.Users.Where(u => u.Id == CreatorId).FirstOrDefaultAsync();
                    }
                    BoardGame game = _context.BoardGames.Where(m => m.GameId == meeting.GameId).First();
                    List<Request> requests = await _context.Requests.Where(r => r.MeetingId == meeting.GameId).ToListAsync();
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

        [Authorize(Roles = "user")]
        public async Task<IActionResult> ShowProfile(int id)
        {
            ApplicationUser tempUser = await _context.Users.Where(i => i.Id == id).FirstOrDefaultAsync();
            var createdMeetings = await GetMeetingCreatorAsync(tempUser);
            var appliedMeetings = await GetMeetingsAsync(tempUser);
            List<Meeting> meetings = new List<Meeting>();
            foreach (var item in createdMeetings)
            {
                meetings.Add(item);
            }
            foreach (var item in appliedMeetings)
            {
                meetings.Add(item);
            }
            meetings.OrderByDescending(d => d.DateOfMeeting);
            List<Meeting> meetingsData = new List<Meeting>();
            int count = 5;
            foreach(var item in meetings)
            {
                if (count <= 0)
                    break;
                else
                {
                    meetingsData.Add(item);
                    count--;
                }
            }
            UserInfo data = new()
            {
                User = tempUser,
                Meetings = meetingsData
            };
            return (View(data));
        }

        private bool MeetingExists(int id)
        {
            return _context.Meetings.Any(e => e.MeetingId == id);
        }


        
       
        [Authorize(Roles = "user")]
        public async Task<IActionResult> RateUser(int? meetingId, long? userId, bool? evaluation)
        {
           
            var user = await _userManager.FindByIdAsync(userId.ToString());
            var request = await _context.Requests.FirstOrDefaultAsync(r => r.UserId == userId && r.MeetingId == meetingId);
            if (evaluation == true)
            {
                user.Rating += 100;

                if (user.Rating > 1000)
                {
                    user.Rating = 1000;
                }


                request.Rated = true;

                
            } else
            {
                user.Rating -= 100;
                if (user.Rating < 0)
                {
                    user.Rating = 0;
                }

                request.Rated = true;
            }

            _context.Update(request);
            await _context.SaveChangesAsync();
            return Redirect("/Meeting/Details/"+meetingId); 


        }

    }
}
