using System;
using System.Collections.Generic;

#nullable disable

namespace Rolling_Tavern.Models
{
    public partial class Request
    {
        public long UserId { get; set; }
        public int MeetingId { get; set; }
        public int StateId { get; set; }

        public bool Rated { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual Meeting Meeting { get; set; }
        public virtual State State { get; set; }
    }
}
