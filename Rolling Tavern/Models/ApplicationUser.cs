using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rolling_Tavern.Models
{
    public class ApplicationUser : IdentityUser<long>
    {
        private string _profilePicture;

        [PersonalData]
        public string FirstName { get; set; }
        
        [PersonalData]
        public string LastName { get; set; }
       
        [PersonalData]
        public DateTime? DateOfBirth { get; set; }

        [PersonalData]
        public int? Rating { get; set; }

        [PersonalData]
        public string ProfilePicture
        {
            get
            {
                if (_profilePicture == null)
                {
                    return "/images/DefaultUser.png";
                }

                return _profilePicture;
            }
            set { _profilePicture = value; }
        }


        [PersonalData]
        public virtual ICollection<Request> Requests { get; set; }

        [PersonalData]
        public virtual ICollection<Meeting> CreatedMeetings { get; set; }


    }
}
