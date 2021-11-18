using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Storage;

#nullable disable

namespace Rolling_Tavern.Models
{
    public partial class Meeting
    { 
        public int MeetingId { get; set; }

        [Required(ErrorMessage = "Поле {0} є обов'язковим.")]
        [StringLength(32, ErrorMessage = "{0} повиннa бути не менше {2} та не більше {1} літер", MinimumLength = 4)]
        [Display(Name = "Назва зустрічі")]
        public string MeetingName { get; set; }

        [Required(ErrorMessage = "{0} повинні бути обрані.")]
        [Display(Name = "Дата та час проведення зустрічі")]
        [DataType(DataType.DateTime)]
        public DateTime DateOfMeeting { get; set; }

        [Required(ErrorMessage = "Поле {0} є обов'язковим.")]
        [Display(Name = "Адреса зустрічі")]
        public string AddresOfMeeting { get; set; }

        [DataType(DataType.Text)]
        [StringLength(1000, ErrorMessage = "{0} повинно бути не більше {1} літер", MinimumLength = 1)]
        [Display(Name = "Опис зустрічі")]
        public string Description { get; set; }

        [DataType(DataType.Text)]
        [StringLength(1000, ErrorMessage = "{0} повинно бути не більше {1} літер", MinimumLength = 1)]
        [Display(Name = "Додаткові вимоги")]
        public string AdditionalRequirements { get; set; }

        [Display(Name = "Фото зустрічі")]
        [AllowedExtensions(new string[] { ".png", ".jpg", ".jpeg", ".gif" })]
        public string PhotoLink { get; set; }
        public long? CreatorId { get; set; }
        public int? GameId { get; set; }
        

        [Display(Name = "Вікові обмеження")]
        [RegularExpression("0?[123456789][0123456789]?", ErrorMessage = "Введіть обмеження віку від 1 до 99")]
        public int? MinimalAge { get; set; }

        public virtual BoardGame Game { get; set; }
        public virtual ApplicationUser Creator { get; set; }

        public virtual ICollection<Request> Requests { get; set; }
        
    }
}
