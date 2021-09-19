using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<AccountController> _logger;
        private UserManager<ApplicationUser> _userManager;
        private SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger)
        {
            _logger = logger;
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
        public async Task<IActionResult> Login(string returnUrl)
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
            model.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user!=null&&user.EmailConfirmed &&
                    (await _userManager.CheckPasswordAsync(user,model.Password)))
                {//判断邮箱是否已经验证
                    ModelState.AddModelError(string.Empty, $"Email not confirmedyet");
                    return View(model);
                }

                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnUrl))
                    {//判断是否有登陆前的页面地址
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
                            EmailConfirmed = true,
                        };
                        await _userManager.CreateAsync(user);

                        //生成电子邮件确认令牌
                        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                        //生成电子邮件的确认链接
                        var confirmationLink = Url.Action("ConfirmEmail", "Account",
                        new { userId = user.Id, token = token }, Request.Scheme);
                        //需要注入ILogger<AccountController> _logger;服务，记录生成的URL链接
                        _logger.Log(LogLevel.Warning, confirmationLink);
                        ViewBag.ErrorTitle = "注册成功";
                        ViewBag.ErrorMessage = $"在你登入系统前,我们已经给您发了一份邮件，需要您先进行邮件验证，点击确认链接即可完成。";
                        return View("Error");

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

        #region 确认电子邮箱
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId,string token)
        {
            if (userId==null&&token==null)
            {
                return RedirectToAction("index", "home");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user==null)
            {
                ViewBag.ErrorMessage = $"当前{userId}无效";
                return View("NotFound");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return View();
            }
            ViewBag.Title = "您的电子邮箱还未经行验证";
            return View("Error");

        }
        #endregion

        #region 重发激活邮件
        [HttpGet]
        public IActionResult ActivateUserEmail()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ActivateUserEmail(EmailAddressViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user!=null)
                {
                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {// 没有激活过的邮箱
                        var getoken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var confirmatioLine = Url.Action("ConfirmEmail", "Account",
                            new { userId = user.Id, token = getoken }, Request.Scheme);
                        // 写入日志
                        _logger.Log(LogLevel.Warning, confirmatioLine);

                        ViewBag.Message = "您在在我们的系统中已经有注册账户，我们已经发送邮件到您的邮箱中，请前往邮箱激活您的账户";
                        return View("ActivateUserEmailConfirmation", ViewBag.Message);
                    }



                }
            }
            ViewBag.Message = "请确认邮箱是否存在异常，现在我们无法给您发送激活链接。";
            return View("ActivateUserEmailConfirmation", ViewBag.Message);
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
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    City = model.City
                };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {//用户数据写入成功
                    //获取邮箱token,并且生成邮箱验证链接
                    var getoken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var confirmatioLine = Url.Action("ConfirmEmail", "Account",
                        new { userId = user.Id, token = getoken }, Request.Scheme);
                    // 写入日志
                    _logger.Log(LogLevel.Warning, confirmatioLine);

                    if (_signInManager.IsSignedIn(User) && User.IsInRole("Admin"))
                    {//如果是管理员创建的用户
                        return RedirectToAction("ListUsers", "Admin");
                    }
                    ViewBag.ErrorTitle = "注册成功";
                    ViewBag.ErrorMessage = $"在你登入系统前,我们已经给您发了一份邮件，需要您先进行邮件验证，点击确认链接即可完成。";
                    return View("Error");

                    //await _signInManager.SignInAsync(user, isPersistent: false);
                    //return RedirectToAction("index", "home");
                }

                //登录失败
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);//失败了走到这里
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
