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
			builder
				.Property(p => p.EventsToBeProcessed)
				.HasConversion(
						v => JsonConvert.SerializeObject(v),
						v => JsonConvert.DeserializeObject<HashSet<EventToBeProcessed>>(v));
			builder
				.Property(p => p.ProcessedEvents)
				.HasConversion(
						v => JsonConvert.SerializeObject(v),
						v => JsonConvert.DeserializeObject<HashSet<ProcessedEvent>>(v));
		}
	}
}
