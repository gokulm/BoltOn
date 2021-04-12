using System.Collections.Generic;
using BoltOn.Cqrs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace BoltOn.Data.EF
{
	public class BaseCqrsEntityMapping<TEntity> : IEntityTypeConfiguration<TEntity>
		where TEntity : BaseDomainEntity
	{
		public virtual void Configure(EntityTypeBuilder<TEntity> builder)
		{
			var settings = new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.All
			};
			builder
				.Property(p => p.EventsToBeProcessed)
				.HasConversion(
						v => JsonConvert.SerializeObject(v, settings),
						v => JsonConvert.DeserializeObject(v, settings) as HashSet<IDomainEvent>);
			builder
				.Property(p => p.ProcessedEvents)
				.HasConversion(
						v => JsonConvert.SerializeObject(v, settings),
						v => JsonConvert.DeserializeObject(v, settings) as HashSet<IDomainEvent>);
		}
	}

	public class EventStore2Mapping : IEntityTypeConfiguration<EventStore>
	{
		public void Configure(EntityTypeBuilder<EventStore> builder)
		{
			builder
				.ToTable("EventStore")
				.HasKey(k => k.EventId);

			builder
				.Property(p => p.EventId)
				.HasColumnName("EventId")
				.ValueGeneratedNever();

			var settings = new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.All
			};
			builder
				.Property(p => p.Data)
				.HasConversion(
						v => JsonConvert.SerializeObject(v, settings),
						v => JsonConvert.DeserializeObject<IDomainEvent>(v, settings));
		}
	}
}


