using MockSchoolManagement.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockSchoolManagement.Application.Departments.Dtos
{
    public class GetDepartmentInput:PagedSortedAndFilterInput
    {
        public GetDepartmentInput()
        {
            Sorting = "Name";
            MaxResultCount = 3;
        }
    }
}
