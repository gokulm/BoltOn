using BoltOn.Data.EF;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BoltOn.Tests.Cqrs
{
    public class TestCqrsWriteEntityMapping : BaseCqrsEntityMapping<TestCqrsWriteEntity>
    {
        public override void Configure(EntityTypeBuilder<TestCqrsWriteEntity> builder)
        {
            base.Configure(builder);
            builder
                .ToTable("TestCqrsWriteEntity")
                .HasKey(k => k.Id);
        }
    }
}
