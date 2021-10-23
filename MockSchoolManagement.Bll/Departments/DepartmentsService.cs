using MockSchoolManagement.Application.Departments.Dtos;
using MockSchoolManagement.Application.Dtos;
using MockSchoolManagement.Application;
using MockSchoolManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MockSchoolManagement.Infrastructure;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;

namespace MockSchoolManagement.Application.Departments
{

    public class DepartmentsService : IDepartmentsService
    {
        private readonly IRepository<Department, int> _departmentRepository;

        public DepartmentsService(IRepository<Department, int> departmentRepository)
        {
            this._departmentRepository = departmentRepository;
        }

        public async Task<PagedResultDto<Department>> GetPageDepartmentsList(GetDepartmentInput input)
        {
            var query = _departmentRepository.GetAll();

            if (!string.IsNullOrEmpty(input.FilterText))//判断
            {
                query = query.Where(a => a.Name.Contains(input.FilterText));
            }

            var count = query.Count();

            query = query.OrderBy(input.Sorting)
                .Skip((input.CurrentPage - 1) * input.MaxResultCount)
                .Take(input.MaxResultCount);

            var models = await query.Include(a => a.Administrator).AsNoTracking().ToListAsync();

            var dtos = new PagedResultDto<Department>
            {
                TotalCount = count,
                CurrentPage=input.CurrentPage,
                MaxResultCount=input.MaxResultCount,
                Data=models,
                FilterText=input.FilterText,
                Sorting=input.Sorting
            };
            return dtos;
        }
    }
}
