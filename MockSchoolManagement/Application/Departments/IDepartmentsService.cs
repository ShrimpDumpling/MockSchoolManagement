using MockSchoolManagement.Application.Dtos;
using MockSchoolManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MockSchoolManagement.Application.Departments.Dtos;

namespace MockSchoolManagement.Application.Departments
{
    public interface IDepartmentsService
    {
        Task<PagedResultDto<Department>> GetPageDepartmentsList(GetDepartmentInput input);
    }
}
