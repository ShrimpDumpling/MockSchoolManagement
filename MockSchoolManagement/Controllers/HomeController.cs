using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockSchoolManagement.Models;
using MockSchoolManagement.DataRepositories;
using MockSchoolManagement.ViewModels;
using System.IO;
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using MockSchoolManagement.Security.CustomTokenProvider;
using MockSchoolManagement.Infrastructure;
using Microsoft.EntityFrameworkCore;
using MockSchoolManagement.Application.Students;
using MockSchoolManagement.Application.Students.Dtos;

namespace MockSchoolManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRepository<Student, int> _studentRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IDataProtector _Protector;
        private readonly IStudentService _studentService;

        public HomeController(IRepository<Student,int> studentRepository, 
            IWebHostEnvironment webHostEnvironment,
            IDataProtectionProvider dataProtector, 
            DataProtectionPurposeStrings dataProtectionPurposeStrings,
            IStudentService studentService)
        {
            _studentRepository = studentRepository;
            _webHostEnvironment = webHostEnvironment;
            _Protector = dataProtector.CreateProtector(
                dataProtectionPurposeStrings.StudentIdRouteValue);
            _studentService = studentService;
        }


        #region 工具方法
        private Student DecryptedStudent(string id)
        {
            Student student;
            try
            {
                //先解密加密过的路由id
                string decryptedId = _Protector.Unprotect(id);
                int decyuptedStudentId = Convert.ToInt32(decryptedId);
                student = _studentRepository.FirstOrDefalult(s => s.Id == decyuptedStudentId);
            }
            catch
            {
                return null;
            }
            return student;
        }
        private async Task<Student> DecryptedStudentAsync(string id)
        {
            Student student;
            try
            {
                //先解密加密过的路由id
                string decryptedId = _Protector.Unprotect(id);
                int decyuptedStudentId = Convert.ToInt32(decryptedId);
                student = await _studentRepository.FirstOrDefalultAsync(s => s.Id == decyuptedStudentId);
            }
            catch
            {
                return null;
            }
            return student;
        }
        #endregion

        [AllowAnonymous]
        public async Task<IActionResult> Index(GetStudentInput input)
        {
            var dtos = await _studentService.GetPaginatedResult(input);
            dtos.Data=dtos.Data
                .Select(s =>
                {//加密了学生ID作为路由放入viewmodel中
                    s.EncryptedId = _Protector.Protect(s.Id.ToString());
                    return s;
                }).ToList();
            return View(dtos);
        }

        #region 学生详情页
        //[Route("details/{id?}")]
        //[Route("home/details/{id?}")]
        [AllowAnonymous]
        public IActionResult Details(string id)
        {
            //Student student;
            //try
            //{
            //    //先解密加密过的路由id
            //    string decryptedId = _Protector.Unprotect(id);
            //    int decyuptedStudentId = Convert.ToInt32(decryptedId);
            //    student = _studentRepository.GetStudent(decyuptedStudentId);
            //}
            //catch
            //{//解密过程抛出Exception的解决方法
            //    ViewBag.ErrorMessage = $"学生Id={id}的信息不存在，请重试";
            //    return View("NotFound");
            //}
            Student student = this.DecryptedStudent(id);
            if (student==null)
            {//什么卵都没查出来
                ViewBag.ErrorMessage = $"学生Id={id}的信息不存在，请重试";
                return View("NotFound");
            }
            var model = new HomeDetailsViewModel()
            {
                PageTitle = "Details Page",
                Student = student
            };
            //if (model.Student==null)
            //{
            //    Response.StatusCode = 404;
            //    return View("StudentNotFound", id);
            //}

            return View(model);
        }
        #endregion

        #region 添加新的学生
        
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        
        [HttpPost]
        public IActionResult Create(StudentCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = null;
                if (model.Photo!=null)
                {//判断是否有新上传图片的逻辑
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "image");
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Photo.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    model.Photo.CopyTo(new FileStream(filePath, FileMode.Create));
                    //model.PhotoPath = uniqueFileName;
                }
                Student student = new Student {
                    Id=model.Id,
                    Name=model.Name,
                    Email=model.Email,
                    PhotoPath=uniqueFileName,
                    Major=model.Major,
                    EnrollmentDate=model.EnrollmentDate
                };


                Student newStudent = _studentRepository.Insert(student);
                return RedirectToAction("index");
                //var encryptedId = _Protector.Protect(newStudent.Id.ToString());
                //return RedirectToAction("Details", new { id = encryptedId });
            }
            return View();
        }
        #endregion

        #region 编辑学生
        
        [HttpGet]
        public IActionResult Edit(string id)
        {
            Student student = this.DecryptedStudent(id);
            if (student == null)
            {
                ViewBag.ErrorMessage = $"编辑错误，请重试";
                return View("NotFound");
            }

            StudentEditViewModel model = new StudentEditViewModel
            {
                Id = student.Id,
                Name = student.Name,
                Email = student.Email,
                EnrollmentDate = student.EnrollmentDate,
                Major = student.Major,
                PhotoPath = student.PhotoPath
            };
            
            return View(model);
        }

        
        [HttpPost]
        public IActionResult Edit(StudentEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                //查询出模型
                var student = _studentRepository.FirstOrDefalult(s => s.Id == model.Id);
                if (student == null)
                {
                    ViewBag.ErrorMessage = $"编辑错误，请重试";
                    return View("NotFound");
                }

                if (model.ExistingPhotoPath!=null)
                {//判断是否有新上传的图片
                    string uniqueFileName = null;
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "image");//合并文件夹的路径
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + model.ExistingPhotoPath.FileName;//生成的文件名
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);//合成文件真实路径
                    model.ExistingPhotoPath.CopyTo(new FileStream(filePath, FileMode.Create));//
                    model.PhotoPath = uniqueFileName;
                }

                student.Name = model.Name;
                student.Email = model.Email;
                student.Major = model.Major;
                student.PhotoPath = model.PhotoPath;
                student.EnrollmentDate = model.EnrollmentDate;


                Student updateStudent = _studentRepository.Update(student);
                return RedirectToAction("index");
                //var encryptedId = _Protector.Protect(student.Id.ToString());
                //return RedirectToAction("Details", new { id = encryptedId });
            }
            return View(model);
        }
        #endregion

        #region 删除学生
        //[Authorize(Policy = "SuperAdminPolicy")]
        public async Task<IActionResult> Remove(string id)
        {
            var student = this.DecryptedStudent(id);
            if (student == null)
            {
                ViewBag.ErrorMessage = $"删除Id={id}失败，请重试";
                return View("NotFound");
            }
            await _studentRepository.DeleteAsync(student);
            return RedirectToAction("Index");
        }
        #endregion
    }
}
