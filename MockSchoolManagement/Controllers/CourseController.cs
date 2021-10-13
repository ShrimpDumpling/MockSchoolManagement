using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MockSchoolManagement.Application.Courses;
using MockSchoolManagement.Application.Courses.Dtos;
using Microsoft.AspNetCore.Mvc.Rendering;
using MockSchoolManagement.Infrastructure;
using MockSchoolManagement.Models;
using Microsoft.EntityFrameworkCore;
using MockSchoolManagement.ViewModels;

namespace MockSchoolManagement.Controllers
{
    public class CourseController:Controller
    {
        private readonly ICourseService _courseService;
        private readonly IRepository<Department, int> _departmentRepository;
        private readonly IRepository<Course, int> _courseRepository;
        private readonly IRepository<CourseAssignment, int> _courseAssignmentRepository;

        public CourseController(ICourseService courseService,
            IRepository<Department, int> departmentRepository,
            IRepository<Course, int> courseRepository,
            IRepository<CourseAssignment, int> courseAssignmentRepository)
        {
            _courseService = courseService;
            _departmentRepository = departmentRepository;
            _courseRepository = courseRepository;
            _courseAssignmentRepository = courseAssignmentRepository;
        }

        public async Task<IActionResult> Index(GetCourseInput input)
        {
            var models = await _courseService.GetPaginatedResult(input);

            return View(models);
        }


        public async Task<ViewResult> Details(int courseId)
        {

            var course = await _courseRepository.GetAll().Include(a => a.Department).FirstOrDefaultAsync(a => a.CourseID == courseId);

            //判断学生信息是否存在
            if (course == null)
            {
                ViewBag.ErrorMessage = $"课程编号{courseId}的信息不存在，请重试。";
                return View("NotFound");
            }

            return View(course);
        }

        #region 添加课程
        [HttpGet]
        public IActionResult Create()
        {
            var dtos = DepartmentsDropDownList();
            CourseCreateViewModel courseCreateViewModel = new CourseCreateViewModel
            {
                DepartmentList = dtos,
            };
            return View(courseCreateViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CourseCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                Course course = new Course
                {
                    CourseID = model.CourseId,
                    Title = model.Title,
                    Credits = model.Credits,
                    DepartmentId = model.DepartmentId
                };

                await _courseRepository.InsertAsync(course);
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        #endregion

        #region 编辑课程信息
        [HttpGet]
        public IActionResult Edit(int? courseId)
        {
            if (!courseId.HasValue)
            {
                ViewBag.ErrorMessage = $"课程编号{courseId}的信息不存在，请重试。";
                return View("NotFound");
            }

            var course = _courseRepository.FirstOrDefalult(a => a.CourseID == courseId);
            if (course==null)
            {
                ViewBag.ErrorMessage = $"课程编号{courseId}的信息不存在，请重试。";
                return View("NotFound");
            }

            var dtos = DepartmentsDropDownList(course.CourseID);
            var model = new CourseCreateViewModel
            {
                DepartmentList = dtos,
                CourseId=course.CourseID,
                Credits=course.Credits,
                Title=course.Title,
                DepartmentId=course.DepartmentId
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(CourseCreateViewModel input)
        {
            if (ModelState.IsValid)
            {
                var course = _courseRepository.FirstOrDefalult(a => a.CourseID == input.CourseId);
                if (course != null)
                {
                    //course.CourseID = input.CourseId;
                    course.Credits = input.Credits;
                    course.DepartmentId = input.DepartmentId;
                    course.Title = input.Title;
                    _courseRepository.Update(course);
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ViewBag.ErrorMessage = $"课程编号{input.CourseId}的信息不存在，请重试。";
                    return View("NotFound");
                }
            }
            return View(input);
        }
        #endregion

        #region 删除课程信息
        [HttpPost]
        public async Task<IActionResult> Remove(int id)
        {
            var model = await _courseRepository.FirstOrDefalultAsync(a => a.CourseID == id);
            if (model==null)
            {
                ViewBag.ErrorMessage = $"课程编号{id}的信息不存在，请重试。";
                return View("NotFound");
            }

            await _courseAssignmentRepository.DeleteAsync(a => a.CourseId == model.CourseID);
            await _courseRepository.DeleteAsync(a => a.CourseID == id);
            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region 工具类方法
        private SelectList DepartmentsDropDownList(object selectedDepartment = null)
        {
            var models = _departmentRepository.GetAll().OrderBy(a => a.Name).AsNoTracking().ToList();
            var dtos = new SelectList(models, "DepartmentId", "Name", "SelectedDepartment");
            return dtos;
        }
        #endregion
    }
}
