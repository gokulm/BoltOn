using System.Collections.Generic;
using System.Text;
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
						v => Encoding.Default.GetBytes(JsonConvert.SerializeObject(v)),
						v => JsonConvert.DeserializeObject<HashSet<CqrsEvent>>(Encoding.Default.GetString(v)));
			builder
				.Property(p => p.ProcessedEvents)
				.HasConversion(
						v => Encoding.Default.GetBytes(JsonConvert.SerializeObject(v)),
						v => JsonConvert.DeserializeObject<HashSet<CqrsEvent>>(Encoding.Default.GetString(v)));
		}
	}
}


