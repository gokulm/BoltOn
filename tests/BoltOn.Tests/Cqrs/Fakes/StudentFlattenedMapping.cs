using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoltOn.Tests.Cqrs.Fakes
{
	public class StudentFlattenedMapping : IEntityTypeConfiguration<StudentFlattened>
    {
        public void Configure(EntityTypeBuilder<StudentFlattened> builder)
        {
            builder
                .ToTable("StudentFlattened")
                .HasKey(k => k.StudentId);
            builder
                .Property(p => p.StudentId)
                .HasColumnName("StudentId")
                .ValueGeneratedNever();
        }
    }
}
