using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rolling_Tavern.Models
{
    public class ApplicationUser : IdentityUser<long>
    {
        [PersonalData]
        public string FirstName { get; set; }
        
        [PersonalData]
        public string LastName { get; set; }
       
        [PersonalData]
        public DateTime? DateOfBirth { get; set; }
        
        [PersonalData]
        public string ProfilePicture { get; set; }

        [PersonalData]
        public virtual ICollection<Request> Meetings { get; set; }

    }
}
