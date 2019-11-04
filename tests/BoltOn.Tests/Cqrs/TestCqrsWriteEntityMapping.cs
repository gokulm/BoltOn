using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BoltOn.Tests.Cqrs
{
    public class TestCqrsWriteEntityMapping : IEntityTypeConfiguration<TestCqrsWriteEntity>
    {
        public void Configure(EntityTypeBuilder<TestCqrsWriteEntity> builder)
        {
            builder
                .ToTable("TestCqrsWriteEntity")
                .HasKey(k => k.Id);
            builder
                .HasMany(p => p.EventsToBeProcessed);
            builder
                .HasMany(p => p.ProcessedEvents);
        }
    }
}
