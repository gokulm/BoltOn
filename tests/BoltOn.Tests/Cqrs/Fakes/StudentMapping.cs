using BoltOn.Data.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoltOn.Tests.Cqrs.Fakes
{
    public class StudentMapping : BaseDomainEntityMapping<Student>
    {
        public override void Configure(EntityTypeBuilder<Student> builder)
        {
            base.Configure(builder);
            builder
                .ToTable("Student")
                .HasKey(k => k.StudentId);
            builder
                 .Property(p => p.StudentId)
                 .HasColumnName("StudentId")
                 .ValueGeneratedNever();
        }
    }
}
