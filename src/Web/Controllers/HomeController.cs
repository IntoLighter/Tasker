using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Web.Models.DTOs;
using Web.Models.VMs;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ITaskBL _taskBL;

        public HomeController(ITaskBL taskBL, IMapper mapper)
        {
            _taskBL = taskBL;
            _mapper = mapper;
        }

        public async Task<IActionResult> Index()
        {
            return View(_mapper.Map<List<TaskForestNodeVM>>(await _taskBL.GetTaskForestAsync(User)));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTask(
            [Bind("Name", "Description", "ListOfExecutors",
                "DateOfRegistration", "DateOfComplete",
                "PlannedComplexity", "ActualExecutionTime")]
            TaskEntity task)
        {
            if (!ModelState.IsValid) return View("Index");
            await _taskBL.AddTaskAsync(task, User);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSubtask(
            [Bind("Name", "Description", "ListOfExecutors",
                "DateOfRegistration", "DateOfComplete",
                "PlannedComplexity", "ActualExecutionTime")]
            TaskEntity task, long parentId)
        {
            if (!ModelState.IsValid) return View("Index");
            await _taskBL.AddSubtaskAsync(task, parentId, User);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveTask(
            [Bind("Id", "Name", "Description",
                "ListOfExecutors", "Status",
                "DateOfRegistration", "DateOfComplete")]
            TaskEntity task)
        {
            if (!ModelState.IsValid) return View("Index");
            await _taskBL.UpdateTaskAsync(task, User);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            await _taskBL.DeleteTaskAsync(id, User);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> GetTask(long id)
        {
            return Ok(_mapper.Map<TaskDTO>(await _taskBL.GetTaskSubtreeAsync(id, User)));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [AllowAnonymous]
        public IActionResult Error()
        {
            return View(new ErrorVM { Exception = HttpContext.Features.Get<IExceptionHandlerPathFeature>()?.Error });
        }
    }
}
