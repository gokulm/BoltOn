using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoltOn.StateMachine
{
    public interface IFiniteStateMachine<TState, TEvent>
    {
        string Transition { get; }
        TState Trigger(TEvent @event);
        TState Trigger<T0>(TEvent @event, T0 t0);
        FiniteStateMachine<TState, TEvent>.InState In(TState state);
        FiniteStateMachine<TState, TEvent>.InStates In(params TState[] states);
        FiniteStateMachine<TState, TEvent> SetCurrentState(TState initialState);
        string GetDotData();
    }

    public partial class FiniteStateMachine<TState, TEvent> : IFiniteStateMachine<TState, TEvent>
    {
        private TState _currentState;
        protected readonly List<InState> _allowedStates = new List<InState>();
        private readonly StringBuilder _transition = new StringBuilder();
        private readonly StringBuilder _dotData = new StringBuilder();

        public FiniteStateMachine()
        {
            _transition.AppendLine("[State] (Event) {Guard}");
        }

        public FiniteStateMachine(TState currentState)
        {
            _currentState = currentState;
        }

        public string Transition => _transition.ToString();

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

        public FiniteStateMachine<TState, TEvent> SetCurrentState(TState initialState)
        {
            _currentState = initialState;
            return this;
        }

        public TState Trigger(TEvent @event)
        {
            var state = _allowedStates.FirstOrDefault(s => s.CurrentState.Equals(_currentState));
            if (state == null)
                throw new Exception($"State not found: {_currentState}");

            _transition.Append($"[{_currentState}] -> ");
            var allowedEvent = state.Events.FirstOrDefault(e => e.CurrentEvent.Equals(@event));
            if (allowedEvent == null)
                throw new Exception($"Current State: {_currentState}. Event not allowed: {@event}");

            _transition.Append($"({@event}) -> ");

            if (allowedEvent.Guard != default(Func<bool>))
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
                _transition.Append($"[{_currentState}]");
            }

            return _currentState;
        }

        public TState Trigger<T0>(TEvent @event, T0 t0)
        {
            var state = _allowedStates.FirstOrDefault(s => s.CurrentState.Equals(_currentState));
            if (state == null)
                throw new Exception("State not found");
            _transition.Append($"[{_currentState}] -> ");
            var allowedEvent = state.Events.FirstOrDefault(e => e.CurrentEvent.Equals(@event));
            if (allowedEvent == null)
                throw new Exception($"{_currentState} Event not allowed: {@event}");

            _transition.Append($"({@event}) -> ");
            var castedEvent = allowedEvent as OnEvent<T0>;
            if (castedEvent.Guard(t0))
            {
                TriggerIfStateEntryEvent(allowedEvent);
            }
            else
            {
                TriggerElseStateEntryEvent(allowedEvent);
            }
            _transition.Append($"[{_currentState}]");
            return _currentState;
        }

        public string GetDotData()
        {
            _dotData.AppendLine("digraph finite_state_machine {");
            _dotData.AppendLine($"node[shape=doublecircle,  color=black]; {_currentState}");
            _dotData.AppendLine("node[shape=circle, color=blue]");

            foreach (var allowedState in _allowedStates)
            {
                foreach (var e in allowedState.Events)
                {
                    if (!e.ToIfState.Equals(default(TState)))
                    {
                        _dotData.AppendLine($"{allowedState.CurrentState} -> {e.ToIfState} [label=\"{e.CurrentEvent} (Yes)\", color=green ]");
                    }

                    if (!e.ToElseState.Equals(default(TEvent)))
                    {
                        _dotData.AppendLine($"{allowedState.CurrentState} -> {e.ToElseState} [label=\"{e.CurrentEvent} (No)\", color=red]");
                    }
                }
            }
            _dotData.AppendLine("}");
            return _dotData.ToString();
        }

        private void TriggerIfStateEntryEvent(OnEvent @event)
        {
            _transition.Append("{Yes} -> ");
            _currentState = @event.ToIfState;
            @event.ThenAction?.Invoke();
        }

        private void TriggerElseStateEntryEvent(OnEvent @event)
        {
            _transition.Append("{No} -> ");
            _currentState = @event.ToElseState;
            if (@event.ElseAction != default)
                @event.ElseAction();
        }
    }
}
