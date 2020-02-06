using Microsoft.Extensions.DependencyInjection;
using BoltOn.Samples.Infrastructure.Data;
using System;
using BoltOn.Bootstrapping;
using BoltOn.Samples.Application.Entities;

namespace BoltOn.Samples.WebApi
{
    public class PostRegistrationTask : IPostRegistrationTask
    {
        private readonly IServiceProvider _serviceProvider;

        public PostRegistrationTask(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Run(PostRegistrationTaskContext context)
        {
			using (var scope = _serviceProvider.CreateScope())
			{
				//var testDbContext = scope.ServiceProvider.GetService<SchoolWriteDbContext>();
				//testDbContext.Database.EnsureDeleted();
				//testDbContext.Database.EnsureCreated();

				//var inState = new StudentType(1, "In-State");
				//var outOfState = new StudentType(2, "Out-of-State");
				//testDbContext.Set<StudentType>().Add(inState);
				//testDbContext.Set<StudentType>().Add(outOfState);
				//testDbContext.SaveChanges();
			}
		}
    }
}
