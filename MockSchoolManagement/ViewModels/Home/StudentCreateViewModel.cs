using Microsoft.AspNetCore.Http;
using MockSchoolManagement.Models.EnumTypes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using MockSchoolManagement.Models;

namespace MockSchoolManagement.ViewModels
{
    public class StudentCreateViewModel
    {
        public int Id { get; set; }

        [Display(Name = "名字")]
        [Required(ErrorMessage = "请输入名字，它不能为空！"),
            MinLength(5, ErrorMessage = "名字的长度不能小于5个字符"),
            MaxLength(50, ErrorMessage = "名字的长度不能超过50个字符")]
        public string Name { get; set; }


        [Required]
        [Display(Name = "主修科目")]
        public MajorEnum Major { get; set; }


        [Display(Name = "电子邮箱")]
        [RegularExpression(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$",
            ErrorMessage = "邮箱的格式不正确")]
        [Required(ErrorMessage = "请输入邮箱地址，它不能为空！")]
        public string Email { get; set; }


        public string PhotoPath { get; set; }


        [Display(Name = "头像")]
        public IFormFile Photo { get; set; } //新图片
        public DateTime EnrollmentDate { get; set; }
    }
}
