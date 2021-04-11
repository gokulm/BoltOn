using System.Collections.Generic;
using BoltOn.Cqrs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace BoltOn.Data.EF
{
	public class BaseCqrsEntityMapping<TEntity> : IEntityTypeConfiguration<TEntity>
		where TEntity : BaseCqrsEntity
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
						v => JsonConvert.DeserializeObject(v, settings) as HashSet<ICqrsEvent>);
			builder
				.Property(p => p.ProcessedEvents)
				.HasConversion(
						v => JsonConvert.SerializeObject(v, settings),
						v => JsonConvert.DeserializeObject(v, settings) as HashSet<ICqrsEvent>);
		}
	}

	public class EventStore2Mapping : IEntityTypeConfiguration<EventStore2>
	{
		public void Configure(EntityTypeBuilder<EventStore2> builder)
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
						v => JsonConvert.DeserializeObject<ICqrsEvent>(v, settings));
		}
	}
}


