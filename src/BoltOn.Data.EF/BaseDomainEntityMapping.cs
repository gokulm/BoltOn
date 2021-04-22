using BoltOn.Cqrs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoltOn.Data.EF
{
	public class BaseDomainEntityMapping<TEntity> : IEntityTypeConfiguration<TEntity>
		where TEntity : BaseDomainEntity
	{
		public virtual void Configure(EntityTypeBuilder<TEntity> builder)
		{
			builder
				.Ignore(p => p.EventsToBeProcessed)
				.Ignore(p => p.PurgeEvents);
		}
	}
}


