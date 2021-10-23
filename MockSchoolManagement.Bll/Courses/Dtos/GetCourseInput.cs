using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MockSchoolManagement.Application.Dtos;

namespace MockSchoolManagement.Application.Courses.Dtos
{
    public class GetCourseInput:PagedSortedAndFilterInput
    {
        public GetCourseInput()
        {
            Sorting = "CourseId";
            MaxResultCount = 5;
        }
    }
}
