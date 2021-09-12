using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    [Authorize(Policy ="AdminRolePolicy")]
    public class AdminController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            ILogger<AdminController> logger)
        {
            this._roleManager = roleManager;
            this._userManager = userManager;
            this._logger = logger;
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

        #region 用户列表首页
        [HttpGet]
        public IActionResult ListUsers()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }
        #endregion

        #region 编辑用户页面
        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"无法找到ID为{id}的用户";
                return View("NotFound");
            }

            var userClaims = await _userManager.GetClaimsAsync(user);
            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                City = user.City,
                Claims = userClaims,
                Roles = userRoles.ToList(),
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user==null)
            {
                ViewBag.ErrorMessager = $"无法找到ID为{model.Id}";
                return View("NotFound");
            }
            else
            {
                user.Email = model.Email;
                user.UserName = model.UserName;
                user.City = model.City;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("ListUsers");
                }
                foreach(var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }

        }
        #endregion

        #region 管理用户关联的角色
        [HttpGet]
        public async Task<IActionResult> ManageUserRoles(string userId)
        {
            ViewBag.user = userId;
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"无法找到ID为的{userId}用户";
                return View("NotFound");
            }

            var model = new List<RolesInUserViewModel>();
            var roles = _roleManager.Roles.ToList();
            foreach (var role in roles)
            {
                var rolesInUserViewModel = new RolesInUserViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name,
                };
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    rolesInUserViewModel.IsSelected = true;
                }
                else
                {
                    rolesInUserViewModel.IsSelected = false;
                }
                model.Add(rolesInUserViewModel);
            }

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ManageUserRoles(List<RolesInUserViewModel> model, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"无法找到ID为{userId}的用户";
                return View("NotFound");
            }
            var roles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, roles);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "无法删除用户中的现有角色");
                return View(model);
            }

            result = await _userManager.AddToRolesAsync(user,
                 model.Where(x => x.IsSelected).Select(y => y.RoleName));

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "无法向用户添加选定的角色");
                return View(model);
            }

            return RedirectToAction("EditUser", new { Id = userId });
        }
        #endregion

        #region 管理用户声明
        [HttpGet]
        public async Task<IActionResult> ManageUserClaims(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"无法找到ID为{userId}的用户";
                return View("NotFound");
            }
            // UserManager服务中的GetClaimsAsync方法获取用户当前的所有声明
            var existingUserClaims = await _userManager.GetClaimsAsync(user);

            var model = new UserClaimsViewModel
            {
                UserId = userId
            };

            // 循环遍历应用程序中的每个声明
            foreach (Claim claim in ClaimsStore.AllClaims)
            {
                UserClaim userClaim = new UserClaim
                {
                    ClaimType = claim.Type
                };

                // 如果用户选中了声明属性，设置IsSelected属性为true， 
                if (existingUserClaims.Any(c => c.Type == claim.Type))
                {
                    userClaim.IsSelected = true;
                }

                model.Claims.Add(userClaim);
            }

            return View(model);

        }

        [HttpPost]
        public async Task<IActionResult> ManageUserClaims(UserClaimsViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                ViewBag.ErrorMessage = $"无法找到ID为{model.UserId}的用户";
                return View("NotFound");
            }
            // //获取所有用户现有的声明并删除它们
            var claims = await _userManager.GetClaimsAsync(user);
            var result = await _userManager.RemoveClaimsAsync(user, claims);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "无法删除当前用户的声明");
                return View(model);
            }
            // 添加界面上选中的所有声明信息
            result = await _userManager.AddClaimsAsync(user,
                model.Claims.Where(c => c.IsSelected).Select(c => new Claim(c.ClaimType,c.IsSelected?"true":"false")));

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "无法向用户添加选定的声明");
                return View(model);
            }
            return RedirectToAction("EditUser", new { Id = model.UserId });
        }
        #endregion

        #region 删除用户
        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user==null)
            {
                ViewBag.ErrorMessage = $"无法找到id为{id}的用户";
                return View("NotFound");
            }
            else
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("ListUsers");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View("Listusers");
            }
        }
        #endregion

        #region 角色列表
        [HttpGet]
        public IActionResult ListRoles()
        {
            var roles = _roleManager.Roles;
            return View(roles);
        }
        #endregion

        #region 创建角色
        
        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        
        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                IdentityRole identityRole = new IdentityRole
                {
                    Name = model.RoleName,
                };
                IdentityResult result = await _roleManager.CreateAsync(identityRole);
                if (result.Succeeded)
                {
                    return RedirectToAction("index", "home");
                }
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }
        #endregion

        #region 编辑角色
        
        [HttpGet]
        public async Task<IActionResult> EditRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                ViewBag.ErrorMessage = @"角色id={id}的信息不存在，请重试。";
                return View("NotFound");
            }
            var model = new EditRoleViewModel
            {
                Id = role.Id,
                RoleName = role.Name
            };
            var users = _userManager.Users.ToList();
            foreach (var user in users)
            {
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    model.Users.Add(user.UserName);
                }
            }

            return View(model);
        }

        
        [HttpPost]
        public async Task<IActionResult> EditRole(EditRoleViewModel model)
        {
            var role = await _roleManager.FindByIdAsync(model.Id);
            if (role == null)
            {
                ViewBag.ErrorMessage = @"角色id={id}的信息不存在，请重试。";
                return View("NotFound");
            }
            else
            {
                role.Name = model.RoleName;

                var result = await _roleManager.UpdateAsync(role);
                if (result.Succeeded)
                {
                    return RedirectToAction("ListRoles");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }
        }
        #endregion

        #region 编辑角色关联的用户
        
        [HttpGet]
        public async Task<IActionResult> EditUsersInRole(string roleId)
        {
            ViewBag.roleId = roleId;

            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"角色{roleId}的信息不存在，请重试";
                return View("NotFound");
            }

            var model = new List<UserRoleViewModel>();
            var users = _userManager.Users.ToList();
            foreach (var user in users)
            {
                var userRoleViewModel = new UserRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName
                };
                var isInrole = await _userManager.IsInRoleAsync(user, role.Name);

                if (isInrole)
                {
                    userRoleViewModel.IsSelected = true;
                }
                else
                {
                    userRoleViewModel.IsSelected = false;
                }
                model.Add(userRoleViewModel);
            }
            return View(model);
        }

        
        [HttpPost]
        public async Task<IActionResult> EditUsersInRole(List<UserRoleViewModel> model, string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                ViewBag.ErrorMessage = $"角色{roleId}的信息不存在，请重试";
                return View("NotFound");
            }

            for (int i = 0; i < model.Count; i++)
            {
                var user = await _userManager.FindByIdAsync(model[i].UserId);
                IdentityResult result = null;
                if (model[i].IsSelected&&!await _userManager.IsInRoleAsync(user,role.Name))
                {
                    result = await _userManager.AddToRoleAsync(user, role.Name);
                }
                else if(!model[i].IsSelected && await _userManager.IsInRoleAsync(user, role.Name))
                {
                    result = await _userManager.RemoveFromRoleAsync(user, role.Name);
                }
                else
                {
                    continue;
                }

                if (result.Succeeded)
                {
                    if (i < (model.Count - 1))
                    {
                        continue;
                    }
                    else
                    {
                        return RedirectToAction("EditRole", new { id = roleId });
                    }
                }
            }
            return RedirectToAction("EditRole", new { id = roleId });
        }
        #endregion

        #region 删除角色
        [HttpPost]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"无法找到id为{id}的角色";
                return View("NotFound");
            }
            else
            {
                try
                {
                    var result = await _roleManager.DeleteAsync(role);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("ListRoles");
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View("ListRoles");
                }
                catch(DbUpdateException ex)
                {
                    _logger.LogError($"发生异常：{ex}");
                    ViewBag.ErrorTitle = $"角色：{role.Name}正在使用中...";
                    ViewBag.ErrorMessage = $"无法删除{role.Name}角色，应为角色中已经存在用户。如果读者想删除次角色，需要先从角色中删除用户，然后尝试删除该角色本身";
                    return View("Error");
                }
            }
        }
        #endregion


    }
}
