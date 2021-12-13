using Rolling_Tavern.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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

        [Required(ErrorMessage = "Поле {0} є обов'язковим.")]
        [StringLength(32, ErrorMessage = "{0} повиннa бути не менше {2} та не більше {1} літер", MinimumLength = 2)]
        [Display(Name = "Назва гри")]
        public string GameName { get; set; }

        [Required(ErrorMessage = "Поле {0} є обов'язковим.")]
        [StringLength(18, ErrorMessage = "{0} повиннa бути не менше {2} та не більше {1} літер", MinimumLength = 4)]
        [Display(Name = "Жанр гри")]
        public string Genre { get; set; }

        [Required(ErrorMessage = "Поле {0} є обов'язковим.")]
        [StringLength(1000, ErrorMessage = "{0} повиннa бути не менше {2} та не більше {1} літер", MinimumLength = 1)]
        [Display(Name = "Опис гри")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Поле {0} є обов'язковим.")]
        [Display(Name = "Мінімальна кількість гравців")]
        [RegularExpression("0?[123456789][0]?", ErrorMessage = "Введіть обмеження по гравцям від 1 до 10")]
        public int MinAmountOfPlayers { get; set; }

        [Required(ErrorMessage = "Поле {0} є обов'язковим.")]
        [Display(Name = "Максимальна кількість гравців")]
        [RegularExpression("0?[123456789][0]?", ErrorMessage = "Введіть обмеження по гравцям від 1 до 10")]
        [GreaterThan(nameof(MinAmountOfPlayers))]
        public int MaxAmountOfPlayers { get; set; }

        [Required(ErrorMessage = "Поле {0} є обов'язковим.")]
        [Display(Name = "Мінімальне вікове обмеження")]
        [RegularExpression("0?[123456789][0123456789]?[0123456789]?", ErrorMessage = "Введіть обмеження по часу гри в хвилинах від 1 до 999")]
        public long MinGameTime { get; set; }

        [Required(ErrorMessage = "Поле {0} є обов'язковим.")]
        [Display(Name = "Максимальне вікове обмеження")]
        [RegularExpression("0?[123456789][0123456789]?[0123456789]?", ErrorMessage = "Введіть обмеження по часу гри в хвилинах від 1 до 999")]
        public long MaxGameTime { get; set; }

        [Display(Name = "Вікові обмеження")]
        [RegularExpression("0?[123456789][0123456789]?", ErrorMessage = "Введіть обмеження віку від 1 до 99")]
        public int? MinAgeOfPlayers { get; set; }

        public virtual ICollection<Meeting> Meetings { get; set; }
        public virtual ICollection<GameImage> Images { get; set; }
    }
}
