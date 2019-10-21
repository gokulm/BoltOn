using BoltOn.Data.EF;
using BoltOn.Samples.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoltOn.Samples.Infrastructure.Data.Mappings
{
	public class StudentMapping : BaseCqrsEntityMapping<Student>
	{
		public override void Configure(EntityTypeBuilder<Student> builder)
		{
			base.Configure(builder);

			builder
				.ToTable("Student")
				.HasKey(k => k.Id);
			builder
				.Property(p => p.Id)
				.HasColumnName("StudentId")
				.ValueGeneratedNever();
		}
	}

	public class StudentTypeMapping : IEntityTypeConfiguration<StudentType>
	{
		public void Configure(EntityTypeBuilder<StudentType> builder)
		{
			builder
				.ToTable("StudentType")
				.HasKey(k => k.Id);
			builder
				.Property(p => p.Id)
				.HasColumnName("StudentTypeId")
				.ValueGeneratedNever();
		}
	}
}
