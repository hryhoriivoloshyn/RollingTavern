using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rolling_Tavern.Data;
using Rolling_Tavern.Models;

namespace Rolling_Tavern.Services
{
    public class MeetingsManager
    {
        public async Task<IEnumerable<Meeting>> GetStoryOfMeetings(ApplicationUser user, ApplicationDbContext context)
        {
            
            var createdMeetings = await GetMeetingCreatorAsync(user, context);
            var appliedMeetings = await GetMeetingsAsync(user, context);
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

            return meetings;
        }
        private async Task<List<Meeting>> GetMeetingCreatorAsync(ApplicationUser user, ApplicationDbContext context)
        {
            long userId = user.Id;
            List<Meeting> data = new List<Meeting>();
            List<Meeting> tempData = await context.Meetings.Where(m => m.CreatorId == userId).ToListAsync();
            if (tempData?.Any() == true)
            {
                foreach (var item in tempData)
                {
                    BoardGame game = context.BoardGames.Where(m => m.GameId == item.GameId).First();
                    List<Request> requests = await context.Requests.Where(r => r.MeetingId == item.MeetingId).ToListAsync();
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
        private async Task<List<Meeting>> GetMeetingsAsync(ApplicationUser user, ApplicationDbContext context )
        {
            long userId = user.Id;

            List<Request> meetingsId = await context.Requests.Where(r => r.UserId == userId && r.StateId == 2).ToListAsync();
            List<Meeting> data = new List<Meeting>();
            if (meetingsId?.Any() == true)
            {
                foreach (var i in meetingsId)
                {
                    Meeting meeting = context.Meetings.Where(m => m.MeetingId == i.MeetingId).First();
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
                        Creator = await context.Users.Where(u => u.Id == CreatorId).FirstOrDefaultAsync();
                    }
                    BoardGame game = context.BoardGames.Where(m => m.GameId == meeting.GameId).First();
                    List<Request> requests = await context.Requests.Where(r => r.MeetingId == meeting.GameId).ToListAsync();
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
    }
}
