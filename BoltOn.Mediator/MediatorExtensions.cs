using System;
using System.Diagnostics.Contracts;
using System.Linq;
using BoltOn.Bootstrapping;

namespace BoltOn.Mediator
{
    public static class MediatorExtensions
    {
        public static void Configure(this Bootstrapper bootstrapper, Action<MediatorOptions> action)
        {
            var options = new MediatorOptions();
			action(options);
			bootstrapper.AddOptions(options);
        }
    }
}
