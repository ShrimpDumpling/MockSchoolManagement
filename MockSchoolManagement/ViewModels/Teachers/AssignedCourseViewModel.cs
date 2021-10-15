using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockSchoolManagement.ViewModels
{
    public class AssignedCourseViewModel
    {
        /// <summary>
        /// 课程ID
        /// </summary>
        public int CourseId { get; set; }
        /// <summary>
        /// 课程标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 是否被选择
        /// </summary>
        public bool IsSelected { get; set; }
    }
}
