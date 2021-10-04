using MockSchoolManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using MockSchoolManagement.Application.Dtos;
using System.Text;
using System.Threading.Tasks;
using MockSchoolManagement.Application.Students.Dtos;

namespace MockSchoolManagement.Application.Students
{
    public interface IStudentService
    {
        Task<PagedResultDto<Student>> GetPaginatedResult(GetStudentInput input);
    }
}
