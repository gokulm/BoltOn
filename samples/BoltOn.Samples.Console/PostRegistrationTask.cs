using System;
using BoltOn.Requestor.Pipeline;
using BoltOn.Samples.Application.Handlers;
using BoltOn.Bootstrapping;
using BoltOn.Samples.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Samples.Console
{
    public class PostRegistrationTask : IPostRegistrationTask
    {
        private readonly IRequestor _requestor;
        private readonly IServiceProvider _serviceProvider;

        public PostRegistrationTask(IRequestor requestor, IServiceProvider serviceProvider)
        {
            _requestor = requestor;
            _serviceProvider = serviceProvider;
        }

        public void Run()
        {
            //using (var scope = _serviceProvider.CreateScope())
            //{
            //    var schoolReadDbContext = scope.ServiceProvider.GetService<SchoolReadDbContext>();
            //    schoolReadDbContext.Database.EnsureDeleted();
            //    schoolReadDbContext.Database.EnsureCreated();
            //}

            //var response = _requestor.ProcessAsync(new PingRequest()).GetAwaiter().GetResult();
            //System.Console.WriteLine(response.Data);
        }
    }
}
