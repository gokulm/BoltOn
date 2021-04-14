using BoltOn.Data.EF;
using BoltOn.Samples.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoltOn.Samples.Infrastructure.Data.Mappings
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
			builder
				.Ignore(p => p.Courses);
		}
	}
}
