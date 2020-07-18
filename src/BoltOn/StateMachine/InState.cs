using System;
using System.Collections.Generic;
using System.Linq;

namespace BoltOn.StateMachine
{
    public partial class FiniteStateMachine<TState, TEvent>
    {
        public class InState
        {
            internal InState(TState state)
            {
                CurrentState = state;
            }

            internal TState CurrentState { get; }
            internal List<OnEvent> Events { get; set; } = new List<OnEvent>();

            public OnEvent On(TEvent @event, Func<bool> guard = default)
            {
                var allowedEvent = Events.FirstOrDefault(e => e.Equals(@event));
                if (allowedEvent == null)
                {
                    allowedEvent = new OnEvent(@event, guard);
                    Events.Add(allowedEvent);
                }

                return allowedEvent;
            }

            public OnEvent On<T0>(TEvent @event, Func<T0, bool> guard)
            {
                var allowedEvent = Events.FirstOrDefault(e => e.Equals(@event));
                if (allowedEvent == null)
                {
                    allowedEvent = new OnEvent<T0>(@event, guard);
                    Events.Add(allowedEvent);
                }

                return allowedEvent;
            }
        }


        public class InStates : List<InState>
        {
            public OnEvents On(TEvent @event, Func<bool> guard = default)
            {
                var onEvents = new OnEvents();
                ForEach(s =>
                {
                    onEvents.Add(s.On(@event, guard));
                });
                return onEvents;
            }

            public OnEvents On<T0>(TEvent @event, Func<T0, bool> guard = default)
            {
                var onEvents = new OnEvents();
                ForEach(s =>
                {
                    onEvents.Add(s.On(@event, guard));
                });
                return onEvents;
            }
        }
    }
}
