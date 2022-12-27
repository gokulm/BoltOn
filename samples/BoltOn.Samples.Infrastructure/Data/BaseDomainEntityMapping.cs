using System;
using BoltOn.Samples.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoltOn.Samples.Infrastructure.Data
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

