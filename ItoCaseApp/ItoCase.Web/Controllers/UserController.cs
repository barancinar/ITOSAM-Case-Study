using ItoCase.Core.DTOs;
using ItoCase.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ItoCase.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _userService.GetAllUsersAsync();
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Roles = await _userService.GetAllRolesAsync();
            return View("Form", new UserCreateDto());
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserCreateDto model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await _userService.GetAllRolesAsync();
                return View("Form", model);
            }

            try
            {
                await _userService.CreateUserAsync(model);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Roles = await _userService.GetAllRolesAsync();
                return View("Form", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);
                
                // UserUpdateDto'ya mapleyelim (Manuel mapping for simplicity here)
                var model = new UserUpdateDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Role = user.Role
                };

                ViewBag.Roles = await _userService.GetAllRolesAsync();
                return View("Form", model); // Aynı formu kullanıyoruz
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UserUpdateDto model)
        {
             if (!ModelState.IsValid)
            {
                ViewBag.Roles = await _userService.GetAllRolesAsync();
                return View("Form", model);
            }

            try
            {
                await _userService.UpdateUserAsync(model);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Roles = await _userService.GetAllRolesAsync();
                return View("Form", model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _userService.DeleteUserAsync(id);
                return Json(new { success = true, message = "Kullanıcı silindi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Silinemedi: " + ex.Message });
            }
        }
    }
}
