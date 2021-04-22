using BoltOn.Samples.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoltOn.Samples.Infrastructure.Data.Mappings
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
                .ValueGeneratedNever();
        }
    }
}
