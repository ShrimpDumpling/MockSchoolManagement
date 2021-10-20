﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using MockSchoolManagement.Models.EnumTypes;
using MockSchoolManagement.ViewModels;

namespace MockSchoolManagement.Models
{
    public class Student:Person
    {
        [Required]
        [Display(Name = "主修科目")]
        public MajorEnum Major { get; set; }

        [Display(Name = "电子邮箱")]
        [RegularExpression(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$",
            ErrorMessage = "邮箱的格式不正确")]
        [Required(ErrorMessage = "请输入邮箱地址，它不能为空！")]
        public string PhotoPath { get; set; }//照片路径


        //这个字段用于Controller加密路由id，不需要储存到数据库里
        [NotMapped]
        public string EncryptedId { get; set; }

        [Display(Name = "入学时间")]
        public DateTime EnrollmentDate { get; set; }//入学时间
        public ICollection<StudentCourse> StudentCourses { get; set; }
    }
}
