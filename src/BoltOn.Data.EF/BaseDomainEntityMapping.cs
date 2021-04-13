using System.Collections.Generic;
using BoltOn.Cqrs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace BoltOn.Data.EF
{
	public class BaseDomainEntityMapping<TEntity> : IEntityTypeConfiguration<TEntity>
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
		}
	}
}


