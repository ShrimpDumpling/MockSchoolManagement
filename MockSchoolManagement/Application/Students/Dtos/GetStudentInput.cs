using MockSchoolManagement.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockSchoolManagement.Application.Students.Dtos
{
    /// <summary>
    /// 获取Student时候需要填写的FilterInput分页数据
    /// </summary>
    public class GetStudentInput:PagedSortedAndFilterInput
    {
        public GetStudentInput()
        {
            Sorting = "Id";
        }
    }
}
