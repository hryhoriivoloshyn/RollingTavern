using System;
using System.Collections.Generic;

#nullable disable

namespace Rolling_Tavern.Models
{
    public partial class Meeting
    {
        public int MeetingId { get; set; }
        public string MeetingName { get; set; }
        public DateTime DateOfMeeting { get; set; }
        public string AddresOfMeeting { get; set; }
        public string Description { get; set; }
        public string AdditionalRequirements { get; set; }
        public string PhotoLink { get; set; }
        public string SponsorId { get; set; }
        public int? GameId { get; set; }

        public virtual BoardGame Game { get; set; }
    }
}
