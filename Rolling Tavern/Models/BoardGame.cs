using System;
using System.Collections.Generic;

#nullable disable

namespace Rolling_Tavern.Models
{
    public partial class BoardGame
    {
        public BoardGame()
        {
            Meetings = new HashSet<Meeting>();
        }

        public int GameId { get; set; }
        public string GameName { get; set; }
        public string Genre { get; set; }
        public string Description { get; set; }
        public int MinAmountOfPlayers { get; set; }
        public int MaxAmountOfPlayers { get; set; }
        public long MinGameTime { get; set; }
        public long MaxGameTime { get; set; }
        public int? MinAgeOfPlayers { get; set; }

        public virtual ICollection<Meeting> Meetings { get; set; }
        public virtual ICollection<GameImage> Images { get; set; }
    }
}
