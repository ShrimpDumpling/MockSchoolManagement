using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace MockSchoolManagement.Models
{
    //扩展的IdentityUser类，添加了城市
    public class ApplicationUser:IdentityUser
    {
        public string City { get; set; }
    }
}
