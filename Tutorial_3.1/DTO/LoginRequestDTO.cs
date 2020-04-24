using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tutorial_3._1.DTO
{
    public class LoginRequestDTO
    {
        public string login { get; set; }
        public string password { get; set; }
        public string refreshToken { get; set; }
    }
}
