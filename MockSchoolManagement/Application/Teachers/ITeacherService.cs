using MockSchoolManagement.Application.Dtos;
using MockSchoolManagement.Application.Teachers.Dtos;
using MockSchoolManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;

namespace MockSchoolManagement.Application.Teachers
{
    public interface ITeacherService
    {
        Task<PagedResultDto<Teacher>> GEtPagedTeacherList(GetTeacherInput input);
    }
}
