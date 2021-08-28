﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MockSchoolManagement.Models.EnumTypes
{
    public enum MajorEnum
    {
        [Display(Name ="未分配")]
        None,
        [Display(Name = "计算机科学")]
        FirestGrade,
        [Display(Name = "电子商务")]
        SecondGrade,
        [Display(Name = "数学")]
        GradeThree
    }
}
