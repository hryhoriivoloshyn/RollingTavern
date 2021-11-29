﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Rolling_Tavern.Data;
using Rolling_Tavern.Models;
using Rolling_Tavern.Services;
using Rolling_Tavern.ViewModel;

namespace Rolling_Tavern.Controllers
{
    [Authorize]
    public class UserProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly MeetingsManager _meetingsManager;

        public UserProfileController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _meetingsManager = new MeetingsManager();
        }

      

        [HttpGet]
        public async Task<IActionResult> Index(long id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user != null)
            {
                var storyOfMeetings = await _meetingsManager.GetStoryOfMeetings(user, _context);
                UserProfileViewModel viewModel = new UserProfileViewModel()
                {
                    User = user,
                    StoryOfMeetings = storyOfMeetings
                };
                return View(viewModel);
            }

            return NotFound();
        }
    }
}
