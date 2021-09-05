using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockSchoolManagement.ViewModels
{
    public class EditUserViewModel
    {
        public EditUserViewModel()
        {
            this.Claims = new List<string>();
            this.Roles = new List<string>();
        }
        public string Id { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string City { get; set; }
        public List<string> Claims { get; set; }
        public List<string> Roles { get; set; }
    }
}
