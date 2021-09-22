using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockSchoolManagement.Security.CustomTokenProvider
{
    public class CustomEmailConfirmationTokenProviderOptions:DataProtectionTokenProviderOptions
    {
        public CustomEmailConfirmationTokenProviderOptions()
        {
            Name = "EmailDataProtectorTokenProvider";
            TokenLifespan = TimeSpan.FromHours(5);
        }
    }
}
