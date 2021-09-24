using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockSchoolManagement.Models;
using MockSchoolManagement.Models.EnumTypes;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace MockSchoolManagement.Infrastructure
{
    public class AppDbContext:IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            :base(options)
        {

        }
        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<StudentCourse> StudentCourses { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);//使用基类里的初始化方法
            modelBuilder.Seed();//自定义种子数据

            var foreigKeys = modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys());
            foreach (var foreignKey in foreigKeys)
            {//删除外键关联
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }

            //自定义生成的数据库表名
            modelBuilder.Entity<Course>().ToTable("Course", "School");
            modelBuilder.Entity<StudentCourse>().ToTable("StudentCourse", "School");
            modelBuilder.Entity<Student>().ToTable("Student", "School");
        }

    }
}
