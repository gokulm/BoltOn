using System;
using BoltOn.Bootstrapping;

namespace BoltOn.Mediator
{
	public static class MediatorExtensions
    {
		public static Bootstrapper ConfigureMediator(this Bootstrapper bootstrapper, Action<MediatorOptions> action)
        {
            var options = new MediatorOptions();
			action(options);
			bootstrapper.AddOptions(options);
			return bootstrapper;
        }
    }
}
