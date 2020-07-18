using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoltOn.StateMachine
{
    public interface IFiniteStateMachine<TState, TEvent>
    {
        TState Trigger(TEvent @event);
        TState Trigger<T0>(TEvent @event, T0 t0);
        FiniteStateMachine<TState, TEvent>.InState In(TState state);
        FiniteStateMachine<TState, TEvent>.InStates In(params TState[] states);
        FiniteStateMachine<TState, TEvent> InitCurrentState(TState initialState);
		string Log { get; }
    }

    public partial class FiniteStateMachine<TState, TEvent> : IFiniteStateMachine<TState, TEvent>
    {
        private TState _currentState;
        protected readonly List<InState> _allowedStates = new List<InState>();
        private readonly StringBuilder _log = new StringBuilder();

        public Dictionary<string, object> Context { get; set; }

		public FiniteStateMachine()
        {
            _log.AppendLine("[State] (Event) {Guard}");
            Context = new Dictionary<string, object>();
        }

        public FiniteStateMachine(TState currentState) : this()
        {
            _currentState = currentState;
        }

        public string Log => _log.ToString();

        public InState In(TState state)
        {
            var allowedState = _allowedStates.FirstOrDefault(s => s.CurrentState.Equals(state));
            if (allowedState == null)
            {
                allowedState = new InState(state);
                _allowedStates.Add(allowedState);
            }

            return allowedState;
        }

        public InStates In(params TState[] states)
        {
            var inStates = new InStates();
            foreach (var state in states)
            {
                var allowedState = _allowedStates.FirstOrDefault(s => s.CurrentState.Equals(state));
                if (allowedState == null)
                {
                    allowedState = new InState(state);
                    _allowedStates.Add(allowedState);
                }
                inStates.Add(allowedState);
            }

            return inStates;
        }

        public FiniteStateMachine<TState, TEvent> InitCurrentState(TState initialState)
        {
            _currentState = initialState;
            return this;
        }

        public TState Trigger(TEvent @event)
        {
            var state = _allowedStates.FirstOrDefault(s => s.CurrentState.Equals(_currentState));
            if (state == null)
                throw new Exception($"State not found: {_currentState}");

            _log.Append($"[{_currentState}] -> ");
            var allowedEvent = state.Events.FirstOrDefault(e => e.CurrentEvent.Equals(@event));
            if (allowedEvent == null)
                throw new Exception($"Current State: {_currentState}. Event not allowed: {@event}");

            _log.Append($"({@event}) -> ");

            if (allowedEvent.Guard != default)
            {
                if (allowedEvent.Guard())
                {
                    TriggerIfStateEntryEvent(allowedEvent);
                }
                else
                {
                    TriggerElseStateEntryEvent(allowedEvent);
                }
            }
            else
            {
                _currentState = allowedEvent.ToIfState;
                TriggerIfStateEntryEvent(allowedEvent);
                _log.Append($"[{_currentState}]");
            }

            return _currentState;
        }

        public TState Trigger<T0>(TEvent @event, T0 t0)
        {
            var state = _allowedStates.FirstOrDefault(s => s.CurrentState.Equals(_currentState));
            if (state == null)
                throw new Exception("State not found");
            _log.Append($"[{_currentState}] -> ");
            var allowedEvent = state.Events.FirstOrDefault(e => e.CurrentEvent.Equals(@event));
            if (allowedEvent == null)
                throw new Exception($"{_currentState} Event not allowed: {@event}");

            _log.Append($"({@event}) -> ");
            var castedEvent = allowedEvent as OnEvent<T0>;
            if (castedEvent.Guard(t0))
            {
                TriggerIfStateEntryEvent(allowedEvent);
            }
            else
            {
                TriggerElseStateEntryEvent(allowedEvent);
            }
            _log.Append($"[{_currentState}]");
            return _currentState;
        }

        private void TriggerIfStateEntryEvent(OnEvent @event)
        {
            _log.Append("{Yes} -> ");
            _currentState = @event.ToIfState;
            @event.ThenAction?.Invoke();
        }

        private void TriggerElseStateEntryEvent(OnEvent @event)
        {
            _log.Append("{No} -> ");
            _currentState = @event.ToElseState;
            if (@event.ElseAction != default)
                @event.ElseAction();
        }
    }
}
