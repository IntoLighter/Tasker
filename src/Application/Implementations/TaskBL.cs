using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Common;
using Application.Exceptions;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TaskStatus = Domain.Enums.TaskStatus;

namespace Application.Implementations
{
    public class TaskBL : ITaskBL
    {
        private readonly IDbContext _context;
        private readonly ILogger<TaskBL> _logger;

        public TaskBL(IDbContext context, ILogger<TaskBL> logger)
        {
            _context = context;
            _logger = logger;
        }

        public Task<List<TaskEntity>> GetTaskForestAsync(ClaimsPrincipal user)
        {
            _logger.LogInformation(LogEvents.GetTaskForest,
                "Getting tasks for user {UserId}", GetUserId(user));
            return
                Task.FromResult(_context.Tasks
                    .Where(e => e.UserId == GetUserId(user))
                    .Include(e => e.ChildrenTasks)
                    .AsEnumerable()
                    .Where(e => e.Parent == null)
                    .ToList());
        }

        public Task<TaskEntity> GetTaskSubtreeAsync(long taskId, ClaimsPrincipal user)
        {
            _logger.LogInformation(LogEvents.GetTaskSubtree,
                "Getting subtree for task {Id} and user {UserId}", taskId, GetUserId(user));
            return Task.FromResult(
                _context.Tasks
                    .Include(e => e.ChildrenTasks)
                    .AsEnumerable()
                    .Single(e => e.Id == taskId));
        }

        public async Task AddTaskAsync(TaskEntity task, ClaimsPrincipal user)
        {
            task.UserId = GetUserId(user);
            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();
            _logger.LogInformation(LogEvents.AddTask,
                "Added task {TaskId} for user {UserId}", task.Id, GetUserId(user));
        }

        public async Task AddSubtaskAsync(TaskEntity task, long parentId, ClaimsPrincipal user)
        {
            task.Parent = await GetTaskSubtreeAsync(parentId, user);
            await AddTaskAsync(task, user);
            TraverseParent(task, node =>
            {
                node.ActualExecutionTime += task.ActualExecutionTime;
                node.PlannedComplexity += task.PlannedComplexity;
            });
            await _context.SaveChangesAsync();
            _logger.LogInformation(LogEvents.AddSubtask,
                "Added subtask {TaskId} for user {UserId}", task.Id, GetUserId(user));
        }

        public async Task DeleteTaskAsync(long id, ClaimsPrincipal user)
        {
            var task = await GetTaskSubtreeAsync(id, user);
            if (task.ChildrenTasks.Count != 0)
            {
                _logger.LogError(LogEvents.TaskNotLeafException,
                    "User {UserId} tried to delete not leaf task {TaskId}", GetUserId(user), id);
                throw new TaskNotLeafException("To be deleted task mustn't have subtasks");
            }

            TraverseParent(task, node =>
            {
                node.ActualExecutionTime -= task.ActualExecutionTime;
                node.PlannedComplexity -= task.PlannedComplexity;
            });
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            _logger.LogInformation(LogEvents.DeleteTask,
                "Deleted task {TaskId} for user {UserId}", id, GetUserId(user));
        }

        public async Task UpdateTaskAsync(TaskEntity updatedTask, ClaimsPrincipal user)
        {
            _logger.LogInformation(LogEvents.UpdateTask,
                "Updating task {TaskId}", updatedTask.Id);
            var originalTask = await GetTaskSubtreeAsync(updatedTask.Id, user);
            var userId = GetUserId(user);
            if (originalTask.Status != updatedTask.Status)
            {
                if (updatedTask.Status == TaskStatus.Paused && originalTask.Status != TaskStatus.Executing)
                {
                    _logger.LogError(LogEvents.TaskNotLeafException,
                        "User {UserId} tried to update status {Status} for task {TaskId} to paused",
                        userId, originalTask.Status, updatedTask.Status);
                    throw new TaskInvalidStatusException("The task should have status executing to be paused");
                }

                if (updatedTask.Status == TaskStatus.Completed)
                    TraverseChildren(originalTask, node =>
                        {
                            if (node.Status != TaskStatus.Executing && node.Status != TaskStatus.Completed)
                            {
                                _logger.LogError(LogEvents.TaskNotLeafException,
                                    "User {UserId} tried to update status for task {TaskId} to completed and " +
                                    "and the task {ChildId} have status {ChildStatus}",
                                    userId, updatedTask.Id, node.Id, node.Status);
                                throw new TaskInvalidStatusException(
                                    "Main and children tasks should have status executing or completed to complete the main task");
                            }

                            node.Status = TaskStatus.Completed;
                        }
                    );
            }

            originalTask.Name = updatedTask.Name;
            originalTask.Description = updatedTask.Description;
            originalTask.ListOfExecutors = updatedTask.ListOfExecutors;
            originalTask.Status = updatedTask.Status;
            originalTask.DateOfRegistration = updatedTask.DateOfRegistration;
            originalTask.DateOfComplete = updatedTask.DateOfComplete;
            await _context.SaveChangesAsync();

            _logger.LogInformation(LogEvents.UpdateTask,
                "Updated task {TaskId} for user {User}", updatedTask.Id, GetUserId(user));
        }

        private static string GetUserId(ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier).Value;
        }

        private static void TraverseParent(TaskEntity node, Action<TaskEntity> callback)
        {
            while (node.Parent != null)
            {
                node = node.Parent;
                callback(node);
            }
        }

        private static void TraverseChildren(TaskEntity node, Action<TaskEntity> callback)
        {
            var queue = new Queue<TaskEntity>();
            queue.Enqueue(node);
            while (queue.Count != 0)
            {
                node = queue.Dequeue();
                callback(node);
                foreach (var child in node.ChildrenTasks) queue.Enqueue(child);
            }
        }
    }
}
