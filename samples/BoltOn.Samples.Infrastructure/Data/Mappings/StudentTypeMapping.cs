using BoltOn.Samples.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoltOn.Samples.Infrastructure.Data.Mappings
{
    public class StudentTypeMapping : IEntityTypeConfiguration<StudentType>
    {
        public void Configure(EntityTypeBuilder<StudentType> builder)
        {
            builder
                .ToTable("StudentType")
                .HasKey(k => k.StudentTypeId);
            builder
                .Property(p => p.StudentTypeId)
                .HasColumnName("StudentTypeId")
                .ValueGeneratedNever();
        }
    }
}
