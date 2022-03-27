using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Common;
using Application.Exceptions;
using Application.Implementations;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using TaskStatus = Domain.Enums.TaskStatus;

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
                // Имя вызывающей функции
                .UseInMemoryDatabase(new StackFrame(1, true).GetMethod()?.Name ?? "db")
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
        public async Task GetTaskForestAsync_TwoTreesWithDifferentUsers_ReturnsSingleTree()
        {
            var context = GetDbContext();
            var bl = GetBL(context);

            context.Tasks.Add(GetTaskWithDefaultUser());
            context.Tasks.Add(new TaskEntity { User = new IdentityUser { Id = "2" } });
            await context.SaveChangesAsync();

            var result = await bl.GetTaskForestAsync(GetClaimsPrincipal());

            Assert.Single(result);
        }

        [Fact]
        public async Task GetTaskForestAsync_TwoTrees_ReturnsTwoTrees()
        {
            var context = GetDbContext();
            var bl = GetBL(context);

            context.Tasks.Add(GetTaskWithDefaultUser());
            context.Tasks.Add(GetTaskWithDefaultUser());
            await context.SaveChangesAsync();

            var result = await bl.GetTaskForestAsync(GetClaimsPrincipal());

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetTaskForestAsync_TreeWithTwoChildren_ReturnsTreeWithBothChildren()
        {
            var context = GetDbContext();
            var bl = GetBL(context);

            context.Tasks.Add(
                new TaskEntity
                {
                    ChildrenTasks = new List<TaskEntity> { GetTaskWithDefaultUser(), GetTaskWithDefaultUser() },
                    User = DefaultUser
                });

            await context.SaveChangesAsync();

            var result = (await bl.GetTaskForestAsync(GetClaimsPrincipal()))[0];

            Assert.Equal(2, result.ChildrenTasks.Count);
            Assert.Equal(result, result.ChildrenTasks[0].Parent);
            Assert.Equal(result, result.ChildrenTasks[1].Parent);
        }

        [Fact]
        public async Task AddSubtaskAsync_TreeWithHeight2_AddsAndUpdatesComputableFields()
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
            await context.SaveChangesAsync();

            await bl.AddSubtaskAsync(new TaskEntity
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
        public async Task DeleteTaskAsync_Tree_DeletesAndUpdatesComputableFields()
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
            await context.SaveChangesAsync();

            await bl.DeleteTaskAsync(subSubTask.Id, GetClaimsPrincipal());

            Assert.Equal(0, rootTask.ActualExecutionTime);
            Assert.Equal(0, subtask.ActualExecutionTime);
            Assert.Equal(0, rootTask.PlannedComplexity);
            Assert.Equal(0, subtask.PlannedComplexity);
        }

        [Fact]
        public async Task UpdateTaskAsync_ValidTree_UpdatesStatues()
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
            await context.SaveChangesAsync();

            await bl.UpdateTaskAsync(new TaskEntity { Id = rootTask.Id, Status = TaskStatus.Completed },
                GetClaimsPrincipal());

            Assert.Equal(TaskStatus.Completed, rootTask.Status);
            Assert.Equal(TaskStatus.Completed, subtask.Status);
        }

        [Fact]
        public async Task UpdateTaskAsync_StatusCompleted_ThrowsTaskInvalidStatusException()
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
                bl.UpdateTaskAsync(new TaskEntity { Id = rootTask.Id, Status = TaskStatus.Completed },
                    GetClaimsPrincipal()));
            await Assert.ThrowsAsync<TaskInvalidStatusException>(() =>
                bl.UpdateTaskAsync(new TaskEntity { Id = subtask.Id, Status = TaskStatus.Completed },
                    GetClaimsPrincipal()));
        }

        [Fact]
        public async Task UpdateTaskAsync_StatusPaused_ThrowsTaskInvalidStatusException()
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