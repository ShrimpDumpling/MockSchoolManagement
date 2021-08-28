using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MockSchoolManagement.Migrations;
using MockSchoolManagement.Models;

namespace MockSchoolManagement.ViewModels
{
    public class StudentEditViewModel:StudentCreateViewModel
    {
        public IFormFile ExistingPhotoPath { get; set; }
        public StudentEditViewModel()
        {

        }
        public StudentEditViewModel(Student student)
        {
            this.Id = student.Id;
            this.Name = student.Name;
            this.Major = student.Major;
            this.Email = student.Email;
            this.PhotoPath = student.PhotoPath;
        }
    }
}
