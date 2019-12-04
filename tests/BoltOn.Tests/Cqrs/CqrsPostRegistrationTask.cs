using System;
using BoltOn.Bootstrapping;
using BoltOn.Cqrs;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Tests.Cqrs
{
    public class CqrsPostRegistrationTask : IPostRegistrationTask
    {
        private readonly IServiceProvider _serviceProvider;

        public CqrsPostRegistrationTask(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Run(PostRegistrationTaskContext context)
        {
            var testDbContext = _serviceProvider.GetService<CqrsDbContext>();
            testDbContext.Database.EnsureDeleted();
            testDbContext.Database.EnsureCreated();

            testDbContext.Set<Student>().Add(new Student
            {
                Id = CqrsConstants.EntityId,
                Name = "value to be replaced"
            });
            testDbContext.Set<StudentFlattened>().Add(new StudentFlattened
			{
                Id = CqrsConstants.EntityId,
                FirstName = "value to be replaced",
                ProcessedEvents = new HashSet<CqrsEvent>
                {
                    new CqrsEvent { Id = Guid.Parse(CqrsConstants.AlreadyProcessedEventId) }
                }
            });
            testDbContext.SaveChanges();
        }
    }
}
