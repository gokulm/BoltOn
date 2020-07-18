using System;
using System.Collections.Generic;

namespace BoltOn.StateMachine
{
	public partial class FiniteStateMachine<TState, TEvent>
	{
		public class OnEvent
		{
			internal OnEvent(TEvent @event, Func<bool> guard = default)
			{
				CurrentEvent = @event;
				Guard = guard;
			}

			internal TEvent CurrentEvent { get; }
			internal TState ToIfState { get; private set; }
			internal TState ToElseState { get; private set; }
			internal Action ThenAction { get; private set; }
			internal Action ElseAction { get; private set; }
			internal Func<bool> Guard { get; }

			public OnEvent Then(TState toIfState, Action thenAction = default)
			{
				ToIfState = toIfState;
				ThenAction = thenAction;
				return this;
			}

			public void Else(TState toElseState, Action elseAction = default)
			{
				ToElseState = toElseState;
				ElseAction = elseAction;
			}
		}

		public class OnEvent<T0> : OnEvent
		{
			internal OnEvent(TEvent @event, Func<T0, bool> guard) : base(@event)
			{
				Guard = guard;
			}

			internal new Func<T0, bool> Guard { get; }
		}

		public class OnEvents : List<OnEvent>
		{
			public OnEvents Then(TState toIfState, Action thenAction = default)
			{
				var onEvents = new OnEvents();
				ForEach(e =>
				{
					onEvents.Add(e.Then(toIfState, thenAction));
				});
				return onEvents;
			}

			public void Else(TState toElseState, Action elseAction = default)
			{
				ForEach(e =>
				{
					e.Else(toElseState, elseAction);
				});
			}
		}

		public class OnEvents<T0> : List<OnEvent<T0>>
		{
			public OnEvents Then(TState toIfState, Action thenAction = default)
			{
				var onEvents = new OnEvents();
				ForEach(e =>
				{
					onEvents.Add(e.Then(toIfState, thenAction));
				});
				return onEvents;
			}

			public void Else(TState toElseState, Action elseAction = default)
			{
				ForEach(e =>
				{
					e.Else(toElseState, elseAction);
				});
			}
		}
	}
}
