using System;
using System.Collections.Generic;
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
    public class BoardGamesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private IWebHostEnvironment _appEnvironment;

        public BoardGamesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment appEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _appEnvironment = appEnvironment;
        }
        public class CurrentState
        {
            public CurrentState()
            {

            }
            public CurrentState(List<BoardGame> games)
            {
                Games = games;
            }
            public List<BoardGame> Games { get; set; }

            public bool Admin { get; set; }
        }

        private async Task<string> UploadPicture(IFormFile gamePicture, BoardGame game, string order)
        {
            const string defaultPicturePath = "/MeetingPictures/DefaultUser.png";
            if (gamePicture == null)
            {
                return defaultPicturePath;
            }
            string format = "Mddyyyyhhmmsstt";
            string imagename = String.Format("{0}", DateTime.Now.ToString(format));
            string pictureType = gamePicture.ContentType;
            string pictureExtension = pictureType.Substring(pictureType.IndexOf("/") + 1);
            string gamePicturePath = "/GamePictures/" + game.GameName[0] + order + imagename + "." + pictureExtension;

            using (var fileStream = new FileStream(_appEnvironment.WebRootPath + gamePicturePath, FileMode.Create))
            {
                await gamePicture.CopyToAsync(fileStream);
            }

            return gamePicturePath;
        }

        public async Task<List<BoardGame>> GetBoardGamesAsync()
        {
            List<BoardGame> tempData = await _context.BoardGames.Where(i => i.GameId != 1).ToListAsync();
            foreach (var game in tempData)
            {
                game.Images = await _context.GameImages.Where(i => i.GameId == game.GameId).ToListAsync();
            }
            return tempData;
        }

        // GET: BoardGames
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var data = await GetBoardGamesAsync();
            CurrentState dataState = new CurrentState();
            dataState.Games = data;
            dataState.Admin = await _userManager.IsInRoleAsync(user, "admin");
            return View(dataState);
        }

        // GET: BoardGames/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var boardGame = await _context.BoardGames
                .FirstOrDefaultAsync(m => m.GameId == id);
            if (boardGame == null)
            {
                return NotFound();
            }

            return View(boardGame);
        }

        // GET: BoardGames/Create
        [Authorize(Roles = "admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: BoardGames/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GameId,GameName,Genre,Description,MinAmountOfPlayers,MaxAmountOfPlayers,MinGameTime,MaxGameTime,MinAgeOfPlayers")] BoardGame boardGame, 
            IFormFile gamePicture1, IFormFile gamePicture2, IFormFile gamePicture3, IFormFile gamePicture4)
        {
            if (ModelState.IsValid)
            {
                if(boardGame.MinAgeOfPlayers==null)
                {
                    boardGame.MinAgeOfPlayers = 6;
                }
                BoardGame newBoardGame = new()
                {
                    GameName = boardGame.GameName,
                    Genre = boardGame.Genre,
                    Description = boardGame.Description,
                    MinAmountOfPlayers = boardGame.MinAmountOfPlayers,
                    MaxAmountOfPlayers = boardGame.MaxAmountOfPlayers,
                    MinGameTime = boardGame.MinGameTime,
                    MaxGameTime = boardGame.MaxGameTime,
                    MinAgeOfPlayers = boardGame.MinAgeOfPlayers
                };
                _context.Add(newBoardGame);
                await _context.SaveChangesAsync();
                BoardGame createdGame = await _context.BoardGames.OrderBy(i=>i.GameId).LastOrDefaultAsync();
                string path1 = null;
                string path2 = null;
                string path3 = null;
                string path4 = null;
                int counter = 0;
                if (gamePicture1 != null)
                {
                    path1 = await UploadPicture(gamePicture1, createdGame, "1");
                    counter++;
                }
                if (gamePicture2 != null)
                {
                    path2 = await UploadPicture(gamePicture2, createdGame, "2");
                    counter++;
                }
                if (gamePicture3 != null)
                {
                    path3 = await UploadPicture(gamePicture3, createdGame, "3");
                    counter++;
                }
                if (gamePicture4 != null)
                {
                    path4 = await UploadPicture(gamePicture4, createdGame, "4");
                    counter++;
                }
                if (counter==0)
                {
                    path1 = await UploadPicture(gamePicture1, createdGame, "1");
                }
                List<string> paths = new List<string>();
                paths.Add(path1);
                paths.Add(path2);
                paths.Add(path3);
                paths.Add(path4);
                foreach (var path in paths)
                {
                    if (path != null)
                    {
                        GameImage newImage = new()
                        {
                            ImagePath = path,
                            GameId = createdGame.GameId
                        };
                        _context.GameImages.Add(newImage);
                    }
                } 
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(boardGame);
        }

        // GET: BoardGames/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var boardGame = await _context.BoardGames.FindAsync(id);
            if (boardGame == null)
            {
                return NotFound();
            }
            return View(boardGame);
        }

        // POST: BoardGames/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("GameId,GameName,Genre,Description,MinAmountOfPlayers,MaxAmountOfPlayers,MinGameTime,MaxGameTime,MinAgeOfPlayers")] BoardGame boardGame)
        {
            if (id != boardGame.GameId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(boardGame);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BoardGameExists(boardGame.GameId))
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
            return View(boardGame);
        }

        // GET: BoardGames/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var boardGame = await _context.BoardGames
                .FirstOrDefaultAsync(m => m.GameId == id);
            if (boardGame == null)
            {
                return NotFound();
            }

            return View(boardGame);
        }

        // POST: BoardGames/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var boardGame = await _context.BoardGames.FindAsync(id);
            List<GameImage> images = await _context.GameImages.Where(i => i.GameId == id).ToListAsync();
            foreach(var image in images)
            {
                _context.GameImages.Remove(image);
                await _context.SaveChangesAsync();
            }
            List<Meeting> meetings = await _context.Meetings.Where(i => i.GameId == id).ToListAsync();
            foreach(var meeting in meetings)
            {
                meeting.GameId = 1;
                await _context.SaveChangesAsync();
            }
            _context.BoardGames.Remove(boardGame);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BoardGameExists(int id)
        {
            return _context.BoardGames.Any(e => e.GameId == id);
        }
    }
}
