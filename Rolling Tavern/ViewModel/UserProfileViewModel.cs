using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rolling_Tavern.Models;

namespace Rolling_Tavern.ViewModel
{
    public class UserProfileViewModel 
    {
        public ApplicationUser User { get; set; }
        public IEnumerable<Meeting> StoryOfMeetings { get; set; }
        public int? MeetingID { get; set; }
    }
}
