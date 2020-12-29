using System;
using BoltOn.Data;
using BoltOn.Data.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;

namespace BoltOn.Tests.Other
{
	public class SchoolDbContext : BaseDbContext<SchoolDbContext>
	{
		public SchoolDbContext(DbContextOptions<SchoolDbContext> options) : base(options)
		{
		}

		protected override void ApplyConfigurations(ModelBuilder modelBuilder)
		{
			modelBuilder.ApplyConfiguration(new StudentMapping());
			modelBuilder.ApplyConfiguration(new AddressMapping());
		}
	}

	public class Student : BaseEntity<int>
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public List<Address> Addresses { get; set; } = new List<Address>();
	}

	public class Address : BaseEntity<Guid>
	{
		public string Street { get; set; }
		public string City { get; set; }
		public Student Student { get; set; }
	}

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
			builder
				.HasMany(p => p.Addresses) 
				.WithOne(p => p.Student);
		}
	}

	public class AddressMapping : IEntityTypeConfiguration<Address>
	{
		public void Configure(EntityTypeBuilder<Address> builder)
		{
			builder
				.ToTable("Address")
				.HasKey(k => k.Id);
			builder
				.Property(p => p.Id)
				.HasColumnName("AddressId");
		}
	}
}
