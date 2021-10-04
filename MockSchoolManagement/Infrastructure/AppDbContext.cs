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
            modelBuilder.Seed();//自定义的方法

            //获取当前系统中所有领域模型上的外键列表
            var foreigKeys = modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys());
            foreach (var foreignKey in foreigKeys)
            {   //然后将它们的删除行为配置为 Restrict即无操作
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }

        }

    }
}
