using Microsoft.Extensions.DependencyInjection;
using BoltOn.Bootstrapping;
using BoltOn.Data.EF;
using Microsoft.EntityFrameworkCore;
using BoltOn.Data;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace BoltOn.Tests.Cqrs
{
    public class TestCqrsRegistrationTask : IRegistrationTask
    {
        public void Run(RegistrationTaskContext context)
        {
            context.Container.AddDbContext<CqrsDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDbCqrsDbContext");
                options.ConfigureWarnings(x => x.Ignore(RelationalEventId.AmbientTransactionWarning));
            });

            context.Container.AddTransient<IRepository<TestCqrsWriteEntity>, Repository<TestCqrsWriteEntity, CqrsDbContext>>();
            context.Container.AddTransient<IRepository<TestCqrsReadEntity>, Repository<TestCqrsReadEntity, CqrsDbContext>>();
        }
    }
}
