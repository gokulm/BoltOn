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

        public static IServiceCollection AddCosmosDbContext<TCosmosDbContext>(this IServiceCollection serviceCollection, Action<CosmosDbOptions> action = null)
        where TCosmosDbContext : BaseCosmosDbContext, new()
        {
            var options = new CosmosDbOptions();
            action?.Invoke(options);

            var instance = new TCosmosDbContext();
            instance.Configure(options);

            serviceCollection.AddSingleton(instance);
            return serviceCollection;
        }
    }
}
