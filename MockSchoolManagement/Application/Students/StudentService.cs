using MockSchoolManagement.Infrastructure;
using MockSchoolManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using MockSchoolManagement.Application.Dtos;
using MockSchoolManagement.Application.Students.Dtos;

namespace MockSchoolManagement.Application.Students
{
    /// <summary>
    /// 根据GetStudentInput的数据返回指定Student列表的方法
    /// </summary>
    public class StudentService : IStudentService
    {
        
        private readonly IRepository<Student, int> _studentRepository;

        public StudentService(IRepository<Student,int> repository)
        {
            _studentRepository = repository;
        }

        /// <summary>
        /// 获取分页数据
        /// </summary>
        public async Task<PageResultDto<Student>> GetPaginatedResult(GetStudentInput input)
        {
            var query = _studentRepository.GetAll();
            if (!string.IsNullOrEmpty(input.FilterText))
            {
                query = query.Where(s => s.Name.Contains(input.FilterText) || s.Email.Contains(input.FilterText));
            }//搜索
            var count = query.Count();
            query = query.OrderBy(input.Sorting).AsNoTracking()
                .Skip(input.MaxResultCount * (input.CurrentPage - 1)).Take(input.MaxResultCount);
            // 排序后分页

            var models = await query.AsNoTracking().ToListAsync();

            var dtos = new PageResultDto<Student>
            {
                TotalCount = count,
                CurrentPage = input.CurrentPage,
                MaxResultCount = input.MaxResultCount,
                Data = models,
                FilterText = input.FilterText,
                Sorting = input.Sorting
            };

            return dtos;

        }
    }
}
