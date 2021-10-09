using MockSchoolManagement.Models.EnumTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockSchoolManagement.Models
{
    public class StudentCourse
    {
        [Key]
        public int StudentsCourseId { get; set; }
        public int CourseID { get; set; }
        public int StudentID { get; set; }
        [DisplayFormat(NullDisplayText ="无成绩")]
        public Grade? Grade { get; set; }
        public Course Course { get; set; }
        public Student Student { get; set; }
    }
}
