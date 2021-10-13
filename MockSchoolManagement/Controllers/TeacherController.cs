using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MockSchoolManagement.Application.Teachers;
using MockSchoolManagement.Application.Teachers.Dtos;
using MockSchoolManagement.ViewModels.Teachers;
using System.Linq.Dynamic.Core;

namespace MockSchoolManagement.Controllers
{
    public class TeacherController:Controller
    {
        private readonly ITeacherService _teacherService;

        public TeacherController(ITeacherService teacherService)
        {
            _teacherService = teacherService;
        }
        
        public async Task<IActionResult> Index(GetTeacherInput input)
        {
            var models = await _teacherService.GEtPagedTeacherList(input);

            var dto = new TeacherListViewModel();

            if (input.Id!=null)
            {
                var teacher = models.Data.FirstOrDefault(a => a.Id == input.Id.Value);
                if (teacher!=null)
                {
                    dto.Courses = teacher.CourseAssignments.Select(a => a.Course).ToList();
                }
                dto.SelectedId = input.Id.Value;
            }

            if (input.CourseId.HasValue)
            {
                var course = dto.Courses.FirstOrDefault(a => a.CourseID == input.CourseId.Value);
                if (course!=null)
                {
                    dto.StudentCourses = course.StudentCourses.ToList();
                }
                dto.SelectedCourseId = input.CourseId.Value;
            }

            dto.Teachers = models;
            return View(dto);
        }


    }
}

