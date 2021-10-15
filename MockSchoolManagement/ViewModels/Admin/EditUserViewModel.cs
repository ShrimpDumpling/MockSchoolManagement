using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MockSchoolManagement.ViewModels
{
    public class EditUserViewModel
    {
        public EditUserViewModel()
        {
            this.Claims = new List<Claim>();
            this.Roles = new List<string>();
        }
        public string Id { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string City { get; set; }
        public IList<Claim> Claims { get; set; }
        public List<string> Roles { get; set; }
    }
}
