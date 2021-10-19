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
using MockSchoolManagement.Infrastructure;
using MockSchoolManagement.Models;
using Microsoft.EntityFrameworkCore;
using MockSchoolManagement.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MockSchoolManagement.Controllers
{
    public class TeacherController:Controller
    {
        private readonly ITeacherService _teacherService;
        private readonly IRepository<Teacher, int> _teacherRepository;
        private readonly IRepository<Course, int> _courseRepository;
        private readonly IRepository<OfficeLocation, int> _officeLocationRepository;
        private readonly IRepository<CourseAssignment, int> _courseAssignmentRepository;

        public TeacherController(ITeacherService teacherService,
            IRepository<Teacher,int> teacherRepository,
            IRepository<Course, int> courseRepository,
            IRepository<CourseAssignment, int> courseAssignmentRepository,
            IRepository<OfficeLocation, int> officeLocationRepository)
        {
            _teacherService = teacherService;
            _teacherRepository = teacherRepository;
            _courseRepository = courseRepository;
            _officeLocationRepository = officeLocationRepository;
            _courseAssignmentRepository = courseAssignmentRepository;
        }
        
        public async Task<IActionResult> Index(GetTeacherInput input)
        {
            var models = await _teacherService.GetPagedTeacherList(input);

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


        #region 添加教师
        [HttpGet]
        public IActionResult Create()
        {
            var allCourses = _courseRepository.GetAllList();
            var viewModel = new List<AssignedCourseViewModel>();
            foreach (var course in allCourses)
            {
                viewModel.Add(new AssignedCourseViewModel
                {
                    CourseId = course.CourseID,
                    Title = course.Title,
                    IsSelected = false
                });
            }
            var dto = new TeacherCreateViewModel();
            dto.AssignedCourse = viewModel;
            return View(dto);
        }
        [HttpPost]
        public async Task<IActionResult> Create(TeacherCreateViewModel input)
        {
            if (ModelState.IsValid)
            {
                var teacher = new Teacher
                {
                    HireDate = input.HireDate,
                    Name = input.Name,
                    OfficeLocations = input.OfficeLocation,
                    CourseAssignments = new List<CourseAssignment>(),
                };

                var courses = input.AssignedCourse.Where(a => a.IsSelected == true).ToList();
                foreach (var course in courses)
                {
                    teacher.CourseAssignments.Add(new CourseAssignment
                    {
                        CourseId = course.CourseId,
                        TeacherId = teacher.Id
                    });
                }
                await _teacherRepository.InsertAsync(teacher);
                return RedirectToAction(nameof(Index));
            }
            return View(input);
        }
        #endregion

        #region 编辑教师功能
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            var model = await _teacherRepository.GetAll().Include(a => a.OfficeLocations)
                .Include(a => a.CourseAssignments).ThenInclude(a => a.Course)
                .AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
            if (model==null)
            {
                ViewBag.ErrorMessage = $"教师信息ID为{id}的信息不存在，请重试。";
                return View("NotFound");
            }
            var dto = new TeacherCreateViewModel
            {
                Id = model.Id,
                Name = model.Name,
                HireDate = model.HireDate,
                OfficeLocation = model.OfficeLocations
            };
            var assignedCourse = AssignedCourseDropDownList(model);
            dto.AssignedCourse = assignedCourse;
            return View(dto);
        }

        [HttpPost,ActionName("Edit")]
        public async Task<IActionResult> EditPost(TeacherCreateViewModel input)
        {
            if (ModelState.IsValid)
            {
                var teacher = await _teacherRepository.GetAll().Include(i => i.OfficeLocations)
                    .Include(i => i.CourseAssignments)
                        .ThenInclude(i => i.Course)
                    .FirstOrDefaultAsync(m => m.Id == input.Id);
                if (teacher==null)
                {
                    ViewBag.ErrorMessage = $"教师信息ID为{input.Id}的信息不存在，请重试。";
                    return View("NotFound");
                }

                teacher.HireDate = input.HireDate;
                teacher.Name = input.Name;
                teacher.OfficeLocations = input.OfficeLocation;
                teacher.CourseAssignments = new List<CourseAssignment>();

                var courses = input.AssignedCourse.Where(a => a.IsSelected);
                foreach (var item in courses)
                {
                    teacher.CourseAssignments.Add(new CourseAssignment
                    {
                        CourseId = item.CourseId,
                        TeacherId = teacher.Id
                    });
                }

                await _teacherRepository.UpdateAsync(teacher);
                return RedirectToActionPermanent(nameof(Index));
            }
            return View(input);
        }
        #endregion

        #region 删除教师功能
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var model = await _teacherRepository.FirstOrDefaultAsync(a => a.Id == id);
            if (model==null)
            {
                ViewBag.ErrorMessage = $"教师id为{id}的信息不存在，请重试";
                return View("NotFound");
            }
            await _officeLocationRepository.DeleteAsync(a => a.TeacherId == model.Id);
            await _courseAssignmentRepository.DeleteAsync(a => a.TeacherId == model.Id);
            await _teacherRepository.DeleteAsync(a => a.Id == model.Id);
            return RedirectToActionPermanent(nameof(Index));
        }
        #endregion

        #region 工具类方法
        private List<AssignedCourseViewModel> AssignedCourseDropDownList(Teacher teacher)
        {
            var allCourse = _courseRepository.GetAll();
            var teacherCourse = new HashSet<int>(teacher.CourseAssignments.Select(c => c.CourseId));
            var viewModel = new List<AssignedCourseViewModel>();
            foreach (var course in allCourse)
            {
                viewModel.Add(new AssignedCourseViewModel
                {
                    CourseId = course.CourseID,
                    Title = course.Title,
                    IsSelected = teacherCourse.Contains(course.CourseID)
                });
            }
            return viewModel;
        }
        #endregion
    }
}

