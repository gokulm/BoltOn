using Microsoft.Extensions.DependencyInjection;
using BoltOn.Samples.Infrastructure.Data;
using System;
using BoltOn.Bootstrapping;
using BoltOn.Samples.Application.Entities;
using System.Linq;

namespace BoltOn.Samples.WebApi
{
    public class PostRegistrationTask : IPostRegistrationTask
    {
        private readonly IServiceProvider _serviceProvider;

        public PostRegistrationTask(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Run()
        {
            using var scope = _serviceProvider.CreateScope();
            var schoolDbContext = scope.ServiceProvider.GetService<SchoolDbContext>();
            //schoolDbContext.Database.EnsureDeleted();
			schoolDbContext.Database.EnsureCreated();

            if (schoolDbContext.Set<StudentType>().Find(1) == null)
            {
                var inState = new StudentType(1, "In-State");
                var outOfState = new StudentType(2, "Out-of-State");
                schoolDbContext.Set<StudentType>().Add(inState);
                schoolDbContext.Set<StudentType>().Add(outOfState);
                schoolDbContext.SaveChanges();
            }

            if (!schoolDbContext.Set<Course>().Any())
            {
                var math = new Course(Guid.Parse("f6ac6329-65a6-48c3-82df-280abda28004"), "Math");
                var physics = new Course(Guid.Parse("2e457208-f99a-41c4-8c0d-2fff3a52af4e"), "Physics");
                schoolDbContext.Set<Course>().Add(math);
                schoolDbContext.Set<Course>().Add(physics);
                schoolDbContext.SaveChanges();
            }
        }
    }
}
