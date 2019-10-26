using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BoltOn.Tests.Cqrs
{
    public class TestCqrsReadEntityMapping : IEntityTypeConfiguration<TestCqrsReadEntity>
    {
        public void Configure(EntityTypeBuilder<TestCqrsReadEntity> builder)
        {
            builder
                .ToTable("TestCqrsReadEntity")
                .HasKey(k => k.Id);
            builder
                .HasMany(p => p.EventsToBeProcessed);
            builder
                .HasMany(p => p.ProcessedEvents);
        }
    }
}
