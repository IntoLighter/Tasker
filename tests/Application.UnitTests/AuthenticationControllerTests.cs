using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Web.AutoMapper;
using Web.Controllers;
using Web.Models.VMs;
using Xunit;

namespace Application.UnitTests
{
    public class AuthenticationControllerTests
    {
        [Fact]
        public async Task LogIn_Good_RedirectsToHomeIndex()
        {
            const string testEmail = "a@a";
            const string testPassword = "1";
            var mockBl = new Mock<IAuthenticationBL>();
            mockBl.Setup(bl => bl.LogInAsync(testEmail,testPassword, null))
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
    }
}
