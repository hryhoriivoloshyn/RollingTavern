using System;
using System.Collections.Generic;

#nullable disable

namespace Rolling_Tavern.Models
{
    public partial class State
    {
        public int StateId { get; set; }
        public string StateImage { get; set; }

        public virtual ICollection<Request> Requests { get; set; }
    }
}
