using System;
using System.Collections.Generic;

namespace BoltOn.Mediator
{
    public class MediatorOptions
    {
        public List<Type> Middlewares { get; set; } = new List<Type>();

        public MediatorOptions()
        {
            Middlewares.Add(typeof(StopwatchMiddleware));
        }
    }
}
