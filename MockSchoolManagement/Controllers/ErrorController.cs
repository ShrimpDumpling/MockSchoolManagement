using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MockSchoolManagement.Controllers
{
    public class ErrorController : Controller
    {
        private ILogger _logger;
        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [AllowAnonymous]
        [Route("Error/{statusCode}")]
        [HttpGet]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            var statusCodeResult =
                HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
            switch (statusCode)
            {
                case 404:
                    ViewBag.ErrorMessage = "抱歉，您访问的页面不存在";
                    //ViewBag.ErrorPath = statusCodeResult.OriginalPath;
                    //ViewBag.QS = statusCodeResult.OriginalQueryString;
                    _logger.LogWarning($"发生一个404错误，路径= "+
                        $"{statusCodeResult.OriginalPath} 以及查询字符串= "+
                        $"{statusCodeResult.OriginalQueryString}");
                    break;
            }
            return View("NotFound");
        }

        [AllowAnonymous]
        public IActionResult Error()
        {
            var excptionHandlePathFeature =
                HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            //ViewBag.ExceptionPath = excptionHandlePathFeature.Path;
            //ViewBag.ExceptionMessage = excptionHandlePathFeature.Error.Message;
            //ViewBag.StackTrace = excptionHandlePathFeature.Error.StackTrace;
            _logger.LogError($"路径{excptionHandlePathFeature.Path} " +
                $"产生了一个错误{excptionHandlePathFeature.Error}");
            return View("Error");
        }
    }
}
