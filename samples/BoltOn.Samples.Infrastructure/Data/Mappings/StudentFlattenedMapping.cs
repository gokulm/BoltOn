using BoltOn.Data.EF;
using BoltOn.Samples.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoltOn.Samples.Infrastructure.Data.Mappings
{
	public class StudentFlattenedMapping : BaseCqrsEntityMapping<StudentFlattened>
    {
        public override void Configure(EntityTypeBuilder<StudentFlattened> builder)
        {
			base.Configure(builder);

            builder
                .ToTable("StudentFlattened")
                .HasKey(k => k.Id);
            builder
                .Property(p => p.Id)
                .HasColumnName("StudentId")
                .ValueGeneratedNever();
		}
    }
}
