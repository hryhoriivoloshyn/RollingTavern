using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rolling_Tavern.Services
{
    public static class RatingManager
    {
        public static string GetRatingImage(int? rating)
        {
            if (rating == null)
            {
                return "";
            }
            if (rating > 500)
            {
                return "/RatingPictures/GoodRating.png";
            }
            else
            {
                return "/RatingPictures/BadRating.png";
            }
           
        }
    }
}
