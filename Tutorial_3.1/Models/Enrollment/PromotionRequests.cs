using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tutorial_3._1.Models.Enrollment
{
    public class PromotionRequests
    {
        public string Studies { get; set; }
        public int Semester { get; set; }

        public bool Check()
        {
            if(string.IsNullOrEmpty(Studies) || string.IsNullOrWhiteSpace(Studies) )
            {
                return false;
            }
            else
            {
                return true;
            }

        }
    }
}
