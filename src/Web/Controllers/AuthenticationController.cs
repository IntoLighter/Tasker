using System.Threading.Tasks;
using Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Web.Models.VMs;

namespace Web.Controllers
{
    [AllowAnonymous]
    public class AuthenticationController : Controller
    {
        private readonly AuthenticationBL _authenticationBL;

        public AuthenticationController(AuthenticationBL authenticationBL)
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
            await _authenticationBL.RegisterAsync(vm.Email, vm.Password);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogIn([Bind("Email", "Password")] AuthenticationVM vm)
        {
            await _authenticationBL.LoginAsync(vm.Email, vm.Password, User);
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> LogOut()
        {
            await _authenticationBL.LogoutAsync(User);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ConfirmEmail(string code, string userId)
        {
            await _authenticationBL.ConfirmEmailAsync(userId, code);
            return User.Identity.IsAuthenticated ? RedirectToAction("Index", "Home") : RedirectToAction("Index");
        }
    }
}
