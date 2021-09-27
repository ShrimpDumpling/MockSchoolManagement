using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MockSchoolManagement.Controllers
{
    public class CourseController:Controller
    {



        public ActionResult Index()
        {
            return View();
        }
    }
}
