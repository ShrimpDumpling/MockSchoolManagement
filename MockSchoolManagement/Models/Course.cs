using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockSchoolManagement.Models
{
    /// <summary>
    /// 课程
    /// </summary>
    public class Course
    {
        /// <summary>
        /// ID不允许自增
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display(Name = "课程编号")]
        public int CourseID { get; set; }
        [Display(Name = "课程名称")]
        public string Title { get; set; }
        [Display(Name = "课程学分")]
        [Range(0,5)]
        public int Credits { get; set; }


        public int DepartmentId { get; set; }
        public Department Department { get; set; }

        public ICollection<CourseAssignment> CourseAssignments { get; set; }
        public ICollection<StudentCourse> StudentCourses { get; set; }
    }
}
