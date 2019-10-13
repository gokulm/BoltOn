using BoltOn.Cqrs;
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
                .HasKey(k => k.Id);
            builder
                .Property(p => p.Id)
                .HasColumnName("StudentId")
                .ValueGeneratedNever();
			builder
				.Ignore(p => p.EventsToBeProcessed);
			builder
				.Ignore(p => p.ProcessedEvents);
			//builder
			//	.HasMany(p => p.EventsToBeProcessed);
			//builder
			//.HasMany(p => p.ProcessedEvents);
		}
    }

	public class EventToBeProcessedMapping : IEntityTypeConfiguration<EventToBeProcessed>
	{
		public void Configure(EntityTypeBuilder<EventToBeProcessed> builder)
		{
			builder
				.ToTable("EventToBeProcessed")
				.HasKey(k => k.Id);
		}
	}

	public class ProcessedEventMapping : IEntityTypeConfiguration<ProcessedEvent>
	{
		public void Configure(EntityTypeBuilder<ProcessedEvent> builder)
		{
			builder
				.ToTable("ProcessedEvent")
				.HasKey(k => k.Id);
		}
	}
}
