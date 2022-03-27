using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface ITaskBL
    {
        public Task<List<TaskEntity>> GetTaskForestAsync(ClaimsPrincipal user);
        public Task<TaskEntity> GetTaskSubtreeAsync(long taskId, ClaimsPrincipal user);
        public Task AddTaskAsync(TaskEntity task, ClaimsPrincipal user);
        public Task AddSubtaskAsync(TaskEntity task, long parentId, ClaimsPrincipal user);
        public Task DeleteTaskAsync(long id, ClaimsPrincipal user);
        public Task UpdateTaskAsync(TaskEntity updatedTask, ClaimsPrincipal user);
    }
}
