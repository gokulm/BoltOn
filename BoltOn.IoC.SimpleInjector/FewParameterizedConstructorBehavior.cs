using System;
using System.Reflection;
using System.Linq;
using SimpleInjector.Advanced;

namespace BoltOn.IoC.SimpleInjector
{
    public class FewParameterizedConstructorBehavior : IConstructorResolutionBehavior
    {
        public ConstructorInfo GetConstructor(Type implementationType) => (
                        from ctor in implementationType.GetConstructors()
                        orderby ctor.GetParameters().Length ascending
                        select ctor)
                    .First();
    }
}
