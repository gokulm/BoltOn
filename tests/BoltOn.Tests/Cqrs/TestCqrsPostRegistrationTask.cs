using System;
using BoltOn.Bootstrapping;
using BoltOn.Cqrs;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Tests.Cqrs
{
    public class TestCqrsPostRegistrationTask : IPostRegistrationTask
    {
        private readonly IServiceProvider _serviceProvider;

        public TestCqrsPostRegistrationTask(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Run(PostRegistrationTaskContext context)
        {
            var testDbContext = _serviceProvider.GetService<CqrsDbContext>();
            testDbContext.Database.EnsureDeleted();
            testDbContext.Database.EnsureCreated();

            testDbContext.Set<TestCqrsWriteEntity>().Add(new TestCqrsWriteEntity
            {
                Id = CqrsConstants.EntityId,
                Input = "value to be replaced"
            });
            testDbContext.Set<TestCqrsReadEntity>().Add(new TestCqrsReadEntity
            {
                Id = CqrsConstants.EntityId,
                Input1 = "value to be replaced",
                ProcessedEvents = new HashSet<CqrsEvent>
                {
                    new CqrsEvent { Id = Guid.Parse(CqrsConstants.AlreadyProcessedEventId) }
                }
            });
            testDbContext.SaveChanges();
        }
    }
}
