using System;
using BoltOn.Mediator.Pipeline;
using BoltOn.Samples.Application.Handlers;
using BoltOn.Bootstrapping;
using BoltOn.Samples.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Samples.Console
{
    public class PostRegistrationTask : IPostRegistrationTask
    {
        private readonly IMediator _mediator;
        private readonly IServiceProvider _serviceProvider;

        public PostRegistrationTask(IMediator mediator, IServiceProvider serviceProvider)
        {
            _mediator = mediator;
            _serviceProvider = serviceProvider;
        }

        public void Run(PostRegistrationTaskContext context)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var schoolReadDbContext = scope.ServiceProvider.GetService<SchoolReadDbContext>();
                schoolReadDbContext.Database.EnsureDeleted();
                schoolReadDbContext.Database.EnsureCreated();
            }

            var response = _mediator.ProcessAsync(new PingRequest()).GetAwaiter().GetResult();
            System.Console.WriteLine(response.Data);
        }
    }
}
