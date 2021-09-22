using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MockSchoolManagement.Models;
using MockSchoolManagement.DataRepositories;
using MockSchoolManagement.ViewModels;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using MockSchoolManagement.Security.CustomTokenProvider;

namespace MockSchoolManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IDataProtector _Protector;

        public HomeController(IStudentRepository studentRepository, 
            IWebHostEnvironment webHostEnvironment,
            IDataProtectionProvider dataProtector, 
            DataProtectionPurposeStrings dataProtectionPurposeStrings)
        {
            _studentRepository = studentRepository;
            _webHostEnvironment = webHostEnvironment;
            _Protector = dataProtector.CreateProtector(
                dataProtectionPurposeStrings.StudentIdRouteValue);
        }


        [AllowAnonymous]
        public IActionResult Index()
        {
            List<Student> model = _studentRepository.GetAllStudents()
                .Select(s=>
                {//加密了学生ID作为路由放入viewmodel中
                    s.EncryptedId = _Protector.Protect(s.Id.ToString());
                    return s;
                }).ToList();

            return View(model);
        }


        #region 学生详情页
        //[Route("details/{id?}")]
        //[Route("home/details/{id?}")]
        [AllowAnonymous]
        public IActionResult Details(string id)
        {
            Student student;
            try
            {
                //先解密加密过的路由id
                string decryptedId = _Protector.Unprotect(id);
                int decyuptedStudentId = Convert.ToInt32(decryptedId);
                student = _studentRepository.GetStudent(decyuptedStudentId);
            }
            catch
            {//解密过程抛出Exception的解决方法
                ViewBag.ErrorMessage = $"学生Id={id}的信息不存在，请重试";
                return View("NotFound");
            }
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
        public IActionResult Create(StudentCreateViewModel student)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = null;
                if (student.Photo!=null)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "image");
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + student.Photo.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    student.Photo.CopyTo(new FileStream(filePath, FileMode.Create));
                    student.PhotoPath = uniqueFileName;
                }
                Student newStudent = _studentRepository.Insert(student);
                return RedirectToAction("Details", new { id = newStudent.Id });
            }
            return View();
        }
        #endregion

        #region 编辑学生
        
        [HttpGet]
        public IActionResult Edit(int id)
        {
            StudentEditViewModel model = 
                new StudentEditViewModel(_studentRepository.GetStudent(id));
            //throw new Exception("Edit出现异常！");
            return View(model);
        }

        
        [HttpPost]
        public IActionResult Edit(StudentEditViewModel student)
        {
            if (ModelState.IsValid)
            {
                if (student.ExistingPhotoPath!=null)
                {
                    string uniqueFileName = null;
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "image");
                    uniqueFileName = Guid.NewGuid().ToString() + "_" + student.ExistingPhotoPath.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    student.ExistingPhotoPath.CopyTo(new FileStream(filePath, FileMode.Create));
                    student.PhotoPath = uniqueFileName;
                }
                Student updateStudent = _studentRepository.Update(student);
                return RedirectToAction("Details", new { id = updateStudent.Id });
            }
            return View();
        }
        #endregion

        #region 删除学生
        //[Authorize(Policy = "SuperAdminPolicy")]
        public IActionResult Remove(int id)
        {
            _studentRepository.Delete(id);
            return RedirectToAction("Index");
        }
        #endregion
    }
}
