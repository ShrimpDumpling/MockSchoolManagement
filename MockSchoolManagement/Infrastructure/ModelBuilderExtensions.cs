using Microsoft.EntityFrameworkCore;
using MockSchoolManagement.Models;
using MockSchoolManagement.Models.EnumTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MockSchoolManagement.Infrastructure
{
    public static class ModelBuilderExtensions
    {
        /// <summary>
        /// 自定义的种子数据
        /// </summary>
        /// <param name="modelBuilder"></param>
        public static void Seed(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>().HasData(
                new Student() { Id = 1, Name = "张三", Major = MajorEnum.FirestGrade, Email = "anonyjie@outlook.com" },
                new Student() { Id = 2, Name = "李四", Major = MajorEnum.FirestGrade, Email = "evilnanako@outlook.com" },
                new Student() { Id = 3, Name = "赵六", Major = MajorEnum.GradeThree, Email = "hackerhzj@qq.com" });
        }
    }
}
