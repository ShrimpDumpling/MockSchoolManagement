﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MockSchoolManagement.Application;
using MockSchoolManagement.Models;
using MockSchoolManagement.ViewModels;
using MockSchoolManagement.Application.Departments;
using MockSchoolManagement.Application.Departments.Dtos;
using MockSchoolManagement.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MockSchoolManagement.Controllers
{
    public class DepartmentsController:Controller
    {
        private readonly IDepartmentsService _departmentsService;
        private readonly IRepository<Department, int> _departmentRepository;
        private readonly IRepository<Teacher, int> _teacherRepository;
        private readonly AppDbContext _dbContext;

        public DepartmentsController(
            IDepartmentsService departmentsService,
            IRepository<Department,int> departmentRepository,
            IRepository<Teacher,int> teacherRepository,
            AppDbContext dbContext)
        {
            _departmentsService = departmentsService;
            _departmentRepository = departmentRepository;
            _teacherRepository = teacherRepository;
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index(GetDepartmentInput input)
        {
            var model = await _departmentsService.GetPageDepartmentsList(input);
            return View(model);
        }
        #region 编辑

        public async Task<IActionResult> Edit(int id)
        {
            var model = await _departmentRepository.GetAll().Include(a => a.Administrator).AsNoTracking()
            .FirstOrDefaultAsync(a => a.DepartmentId == id);
            if (model == null)
            {
                ViewBag.ErrorMessage = $"教师{id}的信息不存在，请重试。";
                return View("NotFound");
            }
            var teacherList = TeachersDropDownList();
            var dto = new DepartmentCreateViewModel
            {
                DepartmentID = model.DepartmentId,
                Name = model.Name,
                Budget = model.Budget,
                StartDate = model.StartDate,
                TeacherID = model.TeacherId,
                Administrator = model.Administrator,
                RowVersion = model.RowVersion,
                TeacherList = teacherList
            };
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(DepartmentCreateViewModel input)
        {
            if (ModelState.IsValid)
            {
                var model = await _departmentRepository.GetAll().Include(a => a.Administrator).FirstOrDefaultAsync(a => a.DepartmentId == input.DepartmentID);

                if (model == null)
                {
                    ViewBag.ErrorMessage = $"教师{input.DepartmentID}的信息不存在，请重试。";
                    return View("NotFound");
                }
                model.DepartmentId = input.DepartmentID;
                model.Name = input.Name;
                model.Budget = input.Budget;
                model.StartDate = input.StartDate;
                model.TeacherId = input.TeacherID;

                //从数据库中获取Department实体中的 RowVersion属性，然后将input.RowVersion赋值到OriginalValue中，EF Core会将两个值进行比较。
                _dbContext.Entry(model).Property("RowVersion").OriginalValue = input.RowVersion;

                try
                {   //UpdateAsync方法执行SaveChangesAsync()方法时，如果检测到并发冲突则会触发DbUpdateConcurrencyException异常
                    await _departmentRepository.UpdateAsync(model);
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {   //异常触发后，获取异常的实体
                    var exceptionEntry = ex.Entries.Single();
                    var clientValues = (Department)exceptionEntry.Entity;//当前值
                    //从数据库中获取该异常实体信息
                    var databaseEntry = exceptionEntry.GetDatabaseValues();//数据库中的值
                    if (databaseEntry == null)
                    {//如果异常实体为null，则表示该行已经被删除
                        ModelState.AddModelError(string.Empty, "无法进行数据的修改。该部门信息已经被其他人所删除！");
                    }
                    else
                    {     //将异常实体中错误信息信息，精确到具体字段程序到视图中。
                        var databaseValues = (Department)databaseEntry.ToObject();
                        if (databaseValues.Name != clientValues.Name)
                            ModelState.AddModelError("Name", $"当前值:{databaseValues.Name}");
                        if (databaseValues.Budget != clientValues.Budget)
                            ModelState.AddModelError("Budget", $"当前值:{databaseValues.Budget}");
                        if (databaseValues.StartDate != clientValues.StartDate)
                            ModelState.AddModelError("StartDate", $"当前值:{databaseValues.StartDate}");
                        if (databaseValues.TeacherId != clientValues.TeacherId)
                        {
                            var teacherEntity =
                                 await _teacherRepository.FirstOrDefaultAsync(a => a.ID == databaseValues.TeacherId);
                            ModelState.AddModelError("TeacherId", $"当前值:{teacherEntity?.Name}");
                        }
                        ModelState.AddModelError("", "你正在编辑的记录已经被其他用户所修改，编辑操作已经被取消，数据库当前的值已经显示在页面上。请再次点击保存。否则请返回列表。");
                        input.RowVersion = databaseValues.RowVersion;
                        //记得初始化老师列表
                        input.TeacherList = TeachersDropDownList();
                        ModelState.Remove("RowVersion");
                    }
                }
            }
            return View(input);
        }

        #endregion 编辑

        #region 添加

        public IActionResult Create()
        {
            var dto = new DepartmentCreateViewModel
            {
                TeacherList = TeachersDropDownList()
            };
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create(DepartmentCreateViewModel input)
        {
            if (ModelState.IsValid)
            {
                Department model = new Department
                {
                    StartDate = input.StartDate,
                    DepartmentId = input.DepartmentID,
                    TeacherId = input.TeacherID,
                    Budget = input.Budget,
                    Name = input.Name,
                };
                await _departmentRepository.InsertAsync(model);
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        #endregion 添加

        #region 删除

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var model = await _departmentRepository.FirstOrDefaultAsync(a => a.DepartmentId == id);

            if (model == null)
            {
                ViewBag.ErrorMessage = $"院系编号{id}的信息不存在，请重试。";
                return View("NotFound");
            }

            await _departmentRepository.DeleteAsync(a => a.DepartmentId == id);
            return RedirectToAction(nameof(Index));
        }

        #endregion 删除

        #region 详情

        public async Task<IActionResult> Details(int Id)
        {
            string query = "SELECT * FROM dbo.Departments WHERE DepartmentID={0}";
            var model = await _dbContext.Departments.FromSqlRaw(query, Id).Include(d => d.Administrator)
                      .AsNoTracking()
                      .FirstOrDefaultAsync();
            if (model == null)
            {
                ViewBag.ErrorMessage = $"部门ID{Id}的信息不存在，请重试。";
                return View("NotFound");
            }

            return View(model);
        }

        #endregion 详情

        /// <summary>
        /// 教师的下拉列表
        /// </summary>
        /// <param name="selectedTeacher"></param>
        private SelectList TeachersDropDownList(object selectedTeacher = null)
        {
            var models = _teacherRepository.GetAll().OrderBy(a => a.Name).AsNoTracking().ToList();
            var dtos = new SelectList(models, "Id", "Name", selectedTeacher);
            return dtos;
        }
    }
}
