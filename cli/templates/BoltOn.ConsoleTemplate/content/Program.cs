using System;
using BoltOn;
using Microsoft.Extensions.DependencyInjection;

namespace BoltOn.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.BoltOn();
            //serviceCollection.AddLogging();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            serviceProvider.TightenBolts();
        }
    }
}
