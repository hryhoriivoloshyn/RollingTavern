using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rolling_Tavern.Models
{
    public partial class GameImage
    {
        public long ImageId { get; set; }
        public string ImagePath { get; set; }

        public int? GameId { get; set; }

        public virtual BoardGame Game { get; set; }
    }
}
