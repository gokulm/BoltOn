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
				TypeNameHandling = TypeNameHandling.Auto
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
}


