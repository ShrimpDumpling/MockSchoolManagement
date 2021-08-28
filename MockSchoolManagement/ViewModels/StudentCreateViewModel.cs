using Microsoft.AspNetCore.Http;
using MockSchoolManagement.Models.EnumTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MockSchoolManagement.Models;

namespace MockSchoolManagement.ViewModels
{
    public class StudentCreateViewModel:Student
    {
        [Display(Name = "头像")]
        public IFormFile Photo { get; set; }
    }
}
