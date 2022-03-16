using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using Application.Common;
using Application.Exceptions;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence.EntityFramework;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Application.UnitTests
{
    public class TaskBLTests
    {
        private static IdentityUser DefaultUser { get; } = new IdentityUser
        {
            Id = "1"
        };

        private static TaskEntity GetTaskWithDefaultUser()
        {
            return new TaskEntity
            {
                User = DefaultUser
            };
        }

        private static IDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(new StackFrame(1, true).GetMethod()?.Name ?? "db") // Имя вызывающей функции
                .Options;
            return new AppDbContext(options);
        }

        private static ClaimsPrincipal GetClaimsPrincipal()
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            }));
        }

        private static TaskBL GetBL(IDbContext context)
        {
            return new TaskBL(context, GetLogger());
        }

        private static ILogger<TaskBL> GetLogger()
        {
            return Mock.Of<ILogger<TaskBL>>();
        }

        [Fact]
        public void GetTaskForestAsync_TwoTreesWithDifferentUsers_ReturnsSingleTree()
        {
            var context = GetDbContext();
            var bl = GetBL(context);

            context.Tasks.Add(GetTaskWithDefaultUser());
            context.Tasks.Add(new TaskEntity { User = new IdentityUser { Id = "2" } });
            context.SaveChangesAsync();

            var result = bl.GetTaskForestAsync(GetClaimsPrincipal()).Result;

            Assert.Single(result);
        }

        [Fact]
        public void GetTaskForestAsync_TwoTrees_ReturnsTwoTrees()
        {
            var context = GetDbContext();
            var bl = GetBL(context);

            context.Tasks.Add(GetTaskWithDefaultUser());
            context.Tasks.Add(GetTaskWithDefaultUser());
            context.SaveChangesAsync();

            var result = bl.GetTaskForestAsync(GetClaimsPrincipal()).Result;

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void GetTaskForestAsync_TreeWithTwoChildren_ReturnsTreeWithBothChildren()
        {
            var context = GetDbContext();
            var bl = GetBL(context);

            context.Tasks.Add(
                new TaskEntity
                {
                    ChildrenTasks = new List<TaskEntity> { GetTaskWithDefaultUser(), GetTaskWithDefaultUser() },
                    User = DefaultUser
                });

            context.SaveChangesAsync();

            var result = bl.GetTaskForestAsync(GetClaimsPrincipal()).Result[0];

            Assert.Equal(2, result.ChildrenTasks.Count);
            Assert.Equal(result, result.ChildrenTasks[0].Parent);
            Assert.Equal(result, result.ChildrenTasks[1].Parent);
        }

        [Fact]
        public void AddSubtaskAsync_TreeWithHeight2_AddsAndUpdatesComputableFields()
        {
            var context = GetDbContext();
            var bl = GetBL(context);

            var rootTask = new TaskEntity
            {
                Id = 1
            };
            var subtask = new TaskEntity
            {
                Id = 2,
                Parent = rootTask
            };

            rootTask.ChildrenTasks = new List<TaskEntity>
            {
                subtask
            };

            context.Tasks.Add(rootTask);
            context.SaveChangesAsync();

            bl.AddSubtaskAsync(new TaskEntity
            {
                ActualExecutionTime = 1,
                PlannedComplexity = 1
            }, subtask.Id, GetClaimsPrincipal());

            Assert.Equal(1, rootTask.ActualExecutionTime);
            Assert.Equal(1, subtask.ActualExecutionTime);
            Assert.Equal(1, rootTask.PlannedComplexity);
            Assert.Equal(1, subtask.PlannedComplexity);
        }

        [Fact]
        public void DeleteTaskAsync_Tree_DeletesAndUpdatesComputableFields()
        {
            var context = GetDbContext();
            var bl = GetBL(context);

            var rootTask = new TaskEntity
            {
                Id = 1,
                ActualExecutionTime = 1,
                PlannedComplexity = 1
            };
            var subtask = new TaskEntity
            {
                Id = 2,
                ActualExecutionTime = 1,
                PlannedComplexity = 1,
                Parent = rootTask
            };
            var subSubTask = new TaskEntity
            {
                Id = 3,
                ActualExecutionTime = 1,
                PlannedComplexity = 1,
                Parent = subtask
            };

            rootTask.ChildrenTasks = new List<TaskEntity>
            {
                subtask
            };
            subtask.ChildrenTasks = new List<TaskEntity>
            {
                subSubTask
            };

            context.Tasks.Add(rootTask);
            context.SaveChangesAsync();

            bl.DeleteTaskAsync(subSubTask.Id, GetClaimsPrincipal());

            Assert.Equal(0, rootTask.ActualExecutionTime);
            Assert.Equal(0, subtask.ActualExecutionTime);
            Assert.Equal(0, rootTask.PlannedComplexity);
            Assert.Equal(0, subtask.PlannedComplexity);
        }

        [Fact]
        public void UpdateTaskAsync_ValidTree_UpdatesStatues()
        {
            var context = GetDbContext();
            var bl = GetBL(context);

            var rootTask = new TaskEntity
            {
                Id = 1,
                Status = TaskStatus.Executing
            };
            var subtask = new TaskEntity
            {
                Id = 2,
                Parent = rootTask,
                Status = TaskStatus.Executing
            };

            rootTask.ChildrenTasks = new List<TaskEntity>
            {
                subtask
            };

            context.Tasks.Add(rootTask);
            context.SaveChangesAsync();

            bl.UpdateTaskAsync(new TaskEntity { Id = rootTask.Id, Status = TaskStatus.Completed }, GetClaimsPrincipal());

            Assert.Equal(TaskStatus.Completed, rootTask.Status);
            Assert.Equal(TaskStatus.Completed, subtask.Status);
        }

        [Fact]
        public async void UpdateTaskAsync_StatusCompleted_ThrowsTaskInvalidStatusException()
        {
            var context = GetDbContext();
            var bl = GetBL(context);

            var rootTask = new TaskEntity
            {
                Status = TaskStatus.Executing
            };
            var subtask = new TaskEntity
            {
                Parent = rootTask,
                Status = TaskStatus.Paused
            };

            rootTask.ChildrenTasks = new List<TaskEntity>
            {
                subtask
            };

            context.Tasks.Add(rootTask);
            await context.SaveChangesAsync();

            await Assert.ThrowsAsync<TaskInvalidStatusException>(() =>
                bl.UpdateTaskAsync(new TaskEntity { Id = rootTask.Id, Status = TaskStatus.Completed }, GetClaimsPrincipal()));
            await Assert.ThrowsAsync<TaskInvalidStatusException>(() =>
                bl.UpdateTaskAsync(new TaskEntity { Id = subtask.Id, Status = TaskStatus.Completed }, GetClaimsPrincipal()));
        }

        [Fact]
        public async void UpdateTaskAsync_StatusPaused_ThrowsTaskInvalidStatusException()
        {
            var context = GetDbContext();
            var bl = GetBL(context);

            var task1 = new TaskEntity
            {
                Id = 1,
                Status = TaskStatus.Assigned
            };
            var task2 = new TaskEntity
            {
                Id = 2,
                Status = TaskStatus.Completed
            };

            context.Tasks.Add(task1);
            context.Tasks.Add(task2);
            await context.SaveChangesAsync();

            await Assert.ThrowsAsync<TaskInvalidStatusException>(() =>
                bl.UpdateTaskAsync(new TaskEntity { Id = task1.Id, Status = TaskStatus.Paused }, GetClaimsPrincipal()));
            await Assert.ThrowsAsync<TaskInvalidStatusException>(() =>
                bl.UpdateTaskAsync(new TaskEntity { Id = task2.Id, Status = TaskStatus.Paused }, GetClaimsPrincipal()));
        }
    }
}
