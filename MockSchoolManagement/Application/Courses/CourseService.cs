using MockSchoolManagement.Application.Courses.Dtos;
using MockSchoolManagement.Application.Dtos;
using MockSchoolManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MockSchoolManagement.Infrastructure;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;

namespace MockSchoolManagement.Application.Courses
{
    public class CourseService : ICourseService
    {
        private readonly IRepository<Course, int> _courseRepository;

        public CourseService(IRepository<Course,int> repository)
        {
            _courseRepository = repository;
        }
        public async Task<PagedResultDto<Course>> GetPaginatedResult(GetCourseInput input)
        {
            var query = _courseRepository.GetAll();
            var count = _courseRepository.Count();

            query = query.OrderBy(input.Sorting)
                .Skip(input.MaxResultCount  * (input.CurrentPage -1)).Take(input.MaxResultCount);

            var models = await query.Include(a=>a.Department).AsNoTracking().ToListAsync();// 预载

            var result = new PagedResultDto<Course>
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
