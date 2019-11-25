using BoltOn.Data.EF;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BoltOn.Tests.Cqrs
{
    public class TestCqrsReadEntityMapping : BaseCqrsEntityMapping<TestCqrsReadEntity>
    {
        public override void Configure(EntityTypeBuilder<TestCqrsReadEntity> builder)
        {
            base.Configure(builder);
            builder
                .ToTable("TestCqrsReadEntity")
                .HasKey(k => k.Id);
        }
    }
}
