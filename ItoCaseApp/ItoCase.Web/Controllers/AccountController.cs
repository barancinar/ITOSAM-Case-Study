using ItoCase.Core.DTOs;
using ItoCase.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ItoCase.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(SignInManager<AppUser> signInManager)
        {
            _signInManager = signInManager;
        }

        // Giriş Sayfası (GET)
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Book");
            }
            return View();
        }

        // Giriş İşlemi (POST)
        [HttpPost]
        public async Task<IActionResult> Login(LoginDto model)
        {
            if (!ModelState.IsValid) return View(model);

            // Kullanıcı adı ve şifre kontrolü (False: Beni hatırla kapalı, False: Kilitlenme kapalı)
            var result = await _signInManager.PasswordSignInAsync(model.UserName!, model.Password!, false, false);

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Book");
            }

            ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı!");
            return View(model);
        }

        // Çıkış İşlemi
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        // Yetkisiz Erişim Sayfası
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}