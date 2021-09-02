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

namespace MockSchoolManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public HomeController(IStudentRepository studentRepository, IWebHostEnvironment webHostEnvironment)
        {
            _studentRepository = studentRepository;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            var model = _studentRepository.GEtAllStudents();
            return View(model);
        }

        //[Route("details/{id?}")]
        //[Route("home/details/{id?}")]
        public IActionResult Details(int? id)
        {
            var model = new HomeDetailsViewModel()
            {
                PageTitle = "Details Page",
                Student = _studentRepository.GetStudent(id??1)
            };
            if (model.Student==null)
            {
                Response.StatusCode = 404;
                return View("StudentNotFound", id);
            }

            return View(model);
        }

        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize]
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
        public IActionResult Remove(int id)
        {
            _studentRepository.Delete(id);
            return RedirectToAction("Index");
        }
    }
}
