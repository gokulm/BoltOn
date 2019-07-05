using System;
using System.Reflection;
using BoltOn.Bootstrapping;
using Microsoft.Extensions.DependencyInjection;
using Pluralize.NET.Core;

namespace BoltOn.Data.CosmosDb
{
    public static class Extensions
    {
        private static Pluralizer _pluralizer;
        
        public static string Pluralize(this string word)
        {
            _pluralizer = _pluralizer ?? new Pluralizer();
            return _pluralizer.Pluralize(word);
        }
        
        public static BoltOnOptions BoltOnCosmosDbModule(this BoltOnOptions boltOnOptions)
        {
            boltOnOptions.BoltOnAssemblies(Assembly.GetExecutingAssembly());
            return boltOnOptions;
        }
        
        //public static IServiceCollection AddCosmosDbContext<TCosmosDbContext>(this IServiceCollection serviceCollection, CosmosDbConfiguration configuration)
        //where TCosmosDbContext : BaseCosmosDbContext, new()
        //{
        //    serviceCollection.AddSingleton(typeof(TCosmosDbContext), svc =>
        //    {
        //        var cosmosDbContext = new TCosmosDbContext();
        //        cosmosDbContext.SetConfiguration(configuration);
        //        return cosmosDbContext;
        //    });
        
        //    var instance = Activator.CreateInstance<TCosmosDbContext>();
        //    instance.SetConfiguration(configuration);
        
        //    serviceCollection.AddSingleton(instance);
        //    return serviceCollection;
        //}
        
        public static IServiceCollection AddCosmosDbContext<TCosmosDbContext>(this IServiceCollection serviceCollection, Action<CosmosDbContextOptions> action)
        where TCosmosDbContext : BaseCosmosDbContext<TCosmosDbContext>
        {
            var options = new CosmosDbContextOptions();
            action(options);
            
            serviceCollection.AddSingleton(svc => new CosmosDbContextOptions<TCosmosDbContext>(options));
            serviceCollection.AddSingleton<TCosmosDbContext>();
            
            return serviceCollection;
        }
    }
}
