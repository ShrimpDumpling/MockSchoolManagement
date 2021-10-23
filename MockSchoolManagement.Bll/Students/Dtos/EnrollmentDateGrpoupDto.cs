using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockSchoolManagement.Application.Students.Dtos
{
    public class EnrollmentDateGrpoupDto
    {
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString ="{0:yyy-MM-dd}",ApplyFormatInEditMode =true)]
        public DateTime? EnrollmentDate { get; set; }
        public int StudentCount { get; set; }
    }
}
