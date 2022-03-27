using System.Threading.Tasks;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Models.VMs;

namespace Web.Controllers
{
    [AllowAnonymous]
    public class AuthenticationController : Controller
    {
        private readonly IAuthenticationBL _authenticationBL;

        public AuthenticationController(IAuthenticationBL authenticationBL)
        {
            _authenticationBL = authenticationBL;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("Email", "Password")] AuthenticationVM vm)
        {
            if (!ModelState.IsValid) return View("Index", vm);
            await _authenticationBL.RegisterAsync(vm.Email, vm.Password, Request);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogIn([Bind("Email", "Password")] AuthenticationVM vm)
        {
            if (!ModelState.IsValid) return View("Index", vm);
            await _authenticationBL.LogInAsync(vm.Email, vm.Password, User);
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> LogOut()
        {
            await _authenticationBL.LogoutAsync(User);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ConfirmEmail(string code, string userId)
        {
            var email = await _authenticationBL.ConfirmEmailAsync(userId, code);
            return View(model: email);
        }
    }
}
