using BoltOn.Data.EF;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BoltOn.Tests.Cqrs.Fakes
{
    public class StudentMapping : BaseCqrsEntityMapping<Student>
    {
        public override void Configure(EntityTypeBuilder<Student> builder)
        {
            base.Configure(builder);
            builder
                .ToTable("Student")
                .HasKey(k => k.CqrsEntityId);
        }
    }
}
