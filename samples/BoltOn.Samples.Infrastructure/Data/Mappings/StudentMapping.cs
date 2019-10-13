using BoltOn.Samples.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoltOn.Samples.Infrastructure.Data.Mappings
{
	public class StudentMapping : IEntityTypeConfiguration<Student>
	{
		public void Configure(EntityTypeBuilder<Student> builder)
		{
			builder
				.ToTable("Student")
				.HasKey(k => k.Id);
			builder
				.Property(p => p.Id)
				.HasColumnName("StudentId")
				.ValueGeneratedNever();
		}
	}

	public class StudentFlattenedMapping : IEntityTypeConfiguration<StudentFlattened>
	{
		public void Configure(EntityTypeBuilder<StudentFlattened> builder)
		{
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
