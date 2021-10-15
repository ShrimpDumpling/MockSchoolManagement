using Microsoft.EntityFrameworkCore;
using MockSchoolManagement.Application.Dtos;
using MockSchoolManagement.Application.Teachers.Dtos;
using MockSchoolManagement.Infrastructure;
using MockSchoolManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;

namespace MockSchoolManagement.Application.Teachers
{
    public class TeacherService : ITeacherService
    {
        private readonly IRepository<Teacher, int> _teacherResitory;

        public TeacherService(IRepository<Teacher,int> teacherRepository)
        {
            _teacherResitory = teacherRepository;
        }
        public async Task<PagedResultDto<Teacher>> GetPagedTeacherList(GetTeacherInput input)
        {
            var query = _teacherResitory.GetAll();

            if (!string.IsNullOrEmpty(input.FilterText))
            {
                query = query.Where(s => s.Name.Contains(input.FilterText));
            }

            var count = query.Count();

            query = query.OrderBy(input.Sorting)
                .Skip(input.MaxResultCount * (input.CurrentPage - 1)).Take(input.MaxResultCount);
            var models = await query.Include(a => a.OfficeLocations)
                .Include(a => a.CourseAssignments)
                    .ThenInclude(a => a.Course)
                    .ThenInclude(a => a.StudentCourses)
                    .ThenInclude(a => a.Student)
                .Include(i => i.CourseAssignments)
                    .ThenInclude(i => i.Course)
                    .ThenInclude(i => i.Department)
                .AsNoTracking().ToListAsync();
            var result = new PagedResultDto<Teacher>
            {
                TotalCount = count,
                CurrentPage = input.CurrentPage,
                MaxResultCount = input.MaxResultCount,
                Data = models,
                FilterText = input.FilterText,
                Sorting = input.Sorting
            };
            return result;
        }
    }
}
