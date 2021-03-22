using BoltOn.Samples.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoltOn.Samples.Infrastructure.Data.Mappings
{
	public class CourseMapping : IEntityTypeConfiguration<Course>
    {
        public void Configure(EntityTypeBuilder<Course> builder)
        {
            builder
                .ToTable("Course")
                .HasKey(k => k.CourseId);
            builder
                .Property(p => p.CourseId)
                .HasColumnName("CourseId")
                .ValueGeneratedNever();
        }
    }
}
