using MockSchoolManagement.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockSchoolManagement.ViewModels
{
    public class TeacherCreateViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name ="姓名")]
        [StringLength(50)]
        public string Name { get; set; }

        [Display(Name = "聘用时间")]
        public DateTime HireDate { get; set; }

        public OfficeLocation OfficeLocation { get; set; }

        public List<AssignedCourseViewModel> AssignedCourse { get; set; }
    }
}
