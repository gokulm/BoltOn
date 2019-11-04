using BoltOn.Data.EF;
using Microsoft.EntityFrameworkCore;

namespace BoltOn.Tests.Cqrs
{
    public class CqrsDbContext : BaseDbContext<CqrsDbContext>
    {
        public CqrsDbContext(DbContextOptions<CqrsDbContext> options) : base(options)
        {
        }

        protected override void ApplyConfigurations(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new TestCqrsWriteEntityMapping());
            modelBuilder.ApplyConfiguration(new TestCqrsReadEntityMapping());
        }
    }
}
