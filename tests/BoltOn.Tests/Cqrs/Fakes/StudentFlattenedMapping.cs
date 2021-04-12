using BoltOn.Data.EF;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace BoltOn.Tests.Cqrs.Fakes
{
    public class StudentFlattenedMapping : BaseCqrsEntityMapping<StudentFlattened>
    {
        public override void Configure(EntityTypeBuilder<StudentFlattened> builder)
        {
            base.Configure(builder);
            builder
                .ToTable("StudentFlattened")
                .HasKey(k => k.DomainEntityId);
        }
    }
}
