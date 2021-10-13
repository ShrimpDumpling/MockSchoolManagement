using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockSchoolManagement.ViewModels
{
    public class CourseCreateViewModel
    {

        public int CourseId { get; set; }

        public string Title { get; set; }

        public int Credits { get; set; }

        public int DepartmentId { get; set; }

        public SelectList DepartmentList { get; set; }


    }
}
