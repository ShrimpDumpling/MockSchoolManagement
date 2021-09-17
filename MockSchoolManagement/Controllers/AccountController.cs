using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MockSchoolManagement.Models;
using MockSchoolManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MockSchoolManagement.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        #region 发生错误
        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            //拒绝访问的页面
            return View();
        }
        #endregion

        #region 登录部分
        [HttpGet]
        public async Task<IActionResult> LoginAsync(string returnUrl)
        {
            LoginViewModel model = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("index", "home");
                    }
                }
                ModelState.AddModelError(string.Empty, "登录失败，请重试");
            }

            return View(model);
        }

        //第三方登录
        [HttpPost]
        public IActionResult ExternalLogin(string provider, string returnUrl)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account",
                                new { ReturnUrl = returnUrl });
            var properties = _signInManager
                .ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        //第三方登录的回调
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null,string remoteError=null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            LoginViewModel loginViewModel = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList(),
            };

            if (remoteError!=null)
            {
                ModelState.AddModelError(string.Empty, $"第三方登录提供程序错误：{remoteError}");
                return View("Login", loginViewModel);
            }

            //从第三方登录提供商获取登录信息
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info==null)
            {
                ModelState.AddModelError(string.Empty, $"加载第三方登录信息出错。");
                return View("Login", loginViewModel);
            }

            //如果之前登录过，表里会有记录，就不需要创建了
            var signInResult = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider,info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (signInResult.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                if (email!=null)
                {
                    //查询用户是否存在
                    var user = await _userManager.FindByEmailAsync(email);
                    if (user==null)
                    {//用户不存在，创建一个用户
                        user = new ApplicationUser
                        {
                            UserName = info.Principal.FindFirstValue(ClaimTypes.Email),
                            Email = info.Principal.FindFirstValue(ClaimTypes.Email),
                        };
                        await _userManager.CreateAsync(user);
                    }
                    await _userManager.AddLoginAsync(user, info);
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }

                ViewBag.ErrorTitle = $"我们无法从提供商：{info.LoginProvider}中解析到读者的邮件地址";
                ViewBag.ErrorMessage = $"请通过联系Anonyjie@outlook.com寻求技术支持。";
                return View("Error");
            }


        }
        #endregion

        #region 判断邮箱是否已经存在
        [AcceptVerbs("Get", "Post")]
        public async Task<IActionResult> IsEmailInUse(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Json(true);
            }
            else
            {
                return Json($"邮箱：{email}已经被注册使用了");
            }
        }
        #endregion

        #region 用户注册
        [HttpGet]
        public IActionResult Register()
        {

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                City = model.City
            };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                if (_signInManager.IsSignedIn(User) && User.IsInRole("Admin"))
                {
                    return RedirectToAction("ListUsers", "Admin");
                }

                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("index", "home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }
        #endregion

        #region 用户注销
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("index", "home");
        }
        #endregion
    }
}
