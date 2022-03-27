using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    public interface IDbContext
    {
        public DbSet<TaskEntity> Tasks { get; set; }
        public Task<int> SaveChangesAsync(CancellationToken token = default);
    }
}
