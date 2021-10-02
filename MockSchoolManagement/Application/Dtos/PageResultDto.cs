using MockSchoolManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockSchoolManagement.Application.Dtos
{
    /// <summary>
    /// 泛型的分页数据返回值
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class PageResultDto<TEntity>:PagedSortedAndFilterInput
    {
        /// <summary>
        /// 数据总合计
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages => (int)Math.Ceiling(decimal.Divide(TotalCount,MaxResultCount));

        /// <summary>
        /// 数据
        /// </summary>
        public List<TEntity> Data { get; set; }

        /// <summary>
        /// 上一页
        /// </summary>
        public bool ShowPrevious => CurrentPage > 1;
        /// <summary>
        /// 下一页
        /// </summary>
        public bool ShowNext => CurrentPage < TotalPages;
        /// <summary>
        /// 第一页
        /// </summary>
        public bool ShowFirst => CurrentPage != 1;
        /// <summary>
        /// 最后一页
        /// </summary>
        public bool ShowLast => CurrentPage != TotalPages;
    }
}
