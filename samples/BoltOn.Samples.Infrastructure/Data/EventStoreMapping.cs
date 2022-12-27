using BoltOn.Samples.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace BoltOn.Samples.Infrastructure.Data
{
	public class EventStoreMapping : IEntityTypeConfiguration<EventStore>
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

