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
            modelBuilder.Entity<Course>().ToTable("Course", "School");
            modelBuilder.Entity<Student>().ToTable("Student", "School");
            modelBuilder.Entity<StudentCourse>().ToTable("StudentCourse", "School");

            modelBuilder.Entity<Department>().ToTable("Department", "School");
            modelBuilder.Entity<Teacher>().ToTable("Teacher", "School");
            modelBuilder.Entity<OfficeLocation>().ToTable("OfficeLocation", "School");
            modelBuilder.Entity<CourseAssignment>().ToTable("CourseAssignment", "School");
        }
    }
}
