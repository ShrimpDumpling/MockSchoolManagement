using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockSchoolManagement.Models
{
    /// <summary>
    /// 学院
    /// </summary>
    public class Department
    {
        public int DepartmentId { get; set; }


        [Display(Name="学院名称")]
        [StringLength(50,MinimumLength =3)]
        public string Name { get; set; }


        [Display(Name = "预算")]
        [DataType(DataType.Currency)]
        [Column(TypeName ="money")]
        public decimal Budget { get; set; }


        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "成立时间")]
        public DateTime StartDate { get; set; }


        [Timestamp]
        public byte[] RowVersion { get; set; }


        [Display(Name = "负责人")]
        public int? TeacherId { get; set; }

        /// <summary>
        /// 学校主任
        /// </summary>
        public Teacher Administrator { get; set; }
        public ICollection<Course> Courses { get; set; }
    }
}
