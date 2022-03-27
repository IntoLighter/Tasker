using System;
using System.Threading.Tasks;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Web.Controllers;
using Web.Models.VMs;
using Xunit;

namespace Web.UnitTests
{
    public class AuthenticationControllerTests
    {
        [Fact]
        public async Task LogIn_Good_RedirectsToHomeIndex()
        {
            const string testEmail = "a@a";
            const string testPassword = "1";
            var mockBl = new Mock<IAuthenticationBL>();
            mockBl.Setup(bl => bl.LogInAsync(testEmail, testPassword, null))
                .Returns(Task.CompletedTask)
                .Verifiable();
            var controller = new AuthenticationController(mockBl.Object);

            var vm = new AuthenticationVM
            {
                Email = testEmail,
                Password = testPassword
            };

            var result = await controller.LogIn(vm);

            mockBl.Verify();
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Home", redirectResult.ControllerName);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task LogIn_ModelStateInvalid_IndexView()
        {
            var mockBl = new Mock<IAuthenticationBL>();
            var controller = new AuthenticationController(mockBl.Object);
            controller.ModelState.AddModelError("Email", "Required");
            controller.ModelState.AddModelError("Password", "Required");

            var result = await controller.LogIn(new AuthenticationVM());

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<AuthenticationVM>(viewResult.Model);
        }

        [Fact]
        public async Task Register_Good_RedirectsToHomeIndex()
        {
            const string testEmail = "a@a";
            const string testPassword = "1";
            var mockBl = new Mock<IAuthenticationBL>();
            mockBl.Setup(bl => bl.RegisterAsync(testEmail, testPassword, null))
                .Returns(Task.CompletedTask)
                .Verifiable();
            var controller = new AuthenticationController(mockBl.Object);

            var vm = new AuthenticationVM
            {
                Email = testEmail,
                Password = testPassword
            };

            var result = await controller.Register(vm);

            mockBl.Verify();
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Home", redirectResult.ControllerName);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task Register_ModelStateInvalid_IndexView()
        {
            var mockBl = new Mock<IAuthenticationBL>();
            var controller = new AuthenticationController(mockBl.Object);
            controller.ModelState.AddModelError("Email", "Required");
            controller.ModelState.AddModelError("Password", "Required");

            var result = await controller.Register(new AuthenticationVM());

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<AuthenticationVM>(viewResult.Model);
        }

        [Fact]
        public async Task Logout_Good_RedirectsToIndex()
        {
            var mockBl = new Mock<IAuthenticationBL>();
            mockBl.Setup(bl => bl.LogoutAsync(null))
                .Returns(Task.CompletedTask)
                .Verifiable();
            var controller = new AuthenticationController(mockBl.Object);

            var result = await controller.LogOut();

            mockBl.Verify();
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Null(redirectResult.ControllerName);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task ConfirmEmail_Good_ViewResult()
        {
            const string userId = "";
            const string code = "";
            const string email = "";
            var mockBl = new Mock<IAuthenticationBL>();
            mockBl.Setup(bl => bl.ConfirmEmailAsync(userId, code))
                .ReturnsAsync(email);
            var controller = new AuthenticationController(mockBl.Object);

            var result = await controller.ConfirmEmail(userId, code);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<string>(viewResult.Model);
            Assert.Equal(email, viewResult.Model);
        }
    }
}
