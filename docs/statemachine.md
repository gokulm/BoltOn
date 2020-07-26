Finite State Machine
--
<blockquote>
A finite-state machine (FSM) or finite-state automaton (FSA, plural: automata), finite automaton, or simply a state machine, is a mathematical model of computation. It is an abstract machine that can be in exactly one of a finite number of states at any given time. The FSM can change from one state to another in response to some inputs; the change from one state to another is called a transition.[1] An FSM is defined by a list of its states, its initial state, and the inputs that trigger each transition.

<p>– <cite><a href="https://en.wikipedia.org/wiki/Finite-state_machine">Wikipedia</a></cite></p>
</blockquote>

To develop a [workflow](https://en.wikipedia.org/wiki/Workflow), we might end up wrting too many if/else and/or switch conditions, which might affect code readability and maintainability when there are too many states, events and transitions, which is where a [State Machine](https://en.wikipedia.org/wiki/Finite-state_machine) will be quite useful.

In order to use BoltOn's [`FiniteStateMachine`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/StateMachine/FiniteStateMachine.cs), you have to do the following:

* Install **BoltOn** NuGet package.
* Declare states and events. Enums will be better (classes can be used too, but see to that you override Equals and GetHashCode methods).

    Like this:

        public enum MusicPlayerState
        {
            Stopped,
            Playing,
            Paused
        }

        public enum MusicPlayerEvent
        {
            Play,
            Stop,
            Pause,
            Previous,
            Next,
            Eject
        }

* Create a class encompassing the state machine with all the state transitions.

    Like this:

        public class MusicPlayerWorkflow
        {
            private FiniteStateMachine<MusicPlayerState, MusicPlayerEvent> _stateMachine;
            public MusicPlayerWorkflow()
            {
                _numberOfSongsInDvd = numberOfSongsInDvd;
                _stateMachine = new FiniteStateMachine<MusicPlayerState, MusicPlayerEvent>()

                _stateMachine
                    .In(MusicPlayerState.Stopped, MusicPlayerState.Paused)
                    .On(MusicPlayerEvent.Play)
                    .Then(MusicPlayerState.Playing);
            }
        }

* Use the following methods to define the state transitions:
    * **In**
    <br />
    It is used to declare the state(s). More than one state could be declared.
    * **On**
    <br />
    It is used to declare the events that can be triggered in the `In` state(s). `On` can be combined with a boolean condition (`Func<bool>` delegate) to enable transition only when the condition is satisfied; when the condition is satisfied, the current state gets set to the one defined in the `Then` state; and if not satisfied, the current state gets set to the one defined in the `Else` state. 

        Like this:

            _stateMachine
                .In(MusicPlayerState.Stopped, MusicPlayerState.Paused)
                .On(MusicPlayerEvent.Play, () => IsDvdInserted())
                .Then(MusicPlayerState.Playing)
                .Else(MusicPlayerState.Stopped);

    * `On` also supports a parameter, which gets passed to the `Func<bool>` delegate. 

        Like this:

            _stateMachine
                .In(MusicPlayerState.Playing)
                .On<int>(MusicPlayerEvent.Next, (currentSongIndex) => currentSongIndex + 1 <= 10)
                .Then(MusicPlayerState.Playing)
                .Else(MusicPlayerState.Stopped);  

    * **Then**
    <br />
    It is used to declare the state that the current state should be set to when an event gets triggered. This should be declared even when `Func<bool>` is not declared in the `On` method. `Then` can be combined with an `Action` delegate, which gets triggered after the state transition. 

        Like this:

            _stateMachine.In(MusicPlayerState.Playing)
                .On<int>(MusicPlayerEvent.Next, (currentSongIndex) => currentSongIndex + 1 <= 10)
                .Then(MusicPlayerState.Playing, () =>
                {
                    _stateMachine.Context["CurrentSongIndex"] = (int)_stateMachine.Context["CurrentSongIndex"] + 1;
                })
                .Else(MusicPlayerState.Stopped); 

    * **Else**
    <br />
    It is used to declare the state that the current state should be set to when an event gets triggered and the `Func<bool>` delegate in the `On` method returns false. `Else` can be combined with an `Action` delegate, which gets triggered after the state transition. 

        Like this:

            _stateMachine.In(MusicPlayerState.Playing)
                .On<int>(MusicPlayerEvent.Next, (currentSongIndex) => currentSongIndex + 1 <= 10)
                .Then(MusicPlayerState.Playing, () =>
                {
                    _stateMachine.Context["CurrentSongIndex"] = (int)_stateMachine.Context["CurrentSongIndex"] + 1;
                })
                .Else(MusicPlayerState.Stopped, () =>
                {
                    _stateMachine.Context["CurrentSongIndex"] = 1;
                })

    * **Trigger**
    <br />
    It is used to trigger events. It also supports a parameter, which gets passed to `On` and which inturn gets passed to the Func<bool> delegate.

        Like this:

            _stateMachine.Trigger(MusicPlayerEvent.Play);

        OR

            _stateMachine
                .In(MusicPlayerState.Playing)
                .On<(int, int)>(MusicPlayerEvent.Next, (c) => c.Item1 + 1 <= c.Item2)
                .Then(MusicPlayerState.Playing, () => currentSongIndex += 1)
                .Else(MusicPlayerState.Playing, () => currentSongIndex = 1);

            _stateMachine.Trigger(MusicPlayerEvent.Next, (inputCurrentSongIndex, numberOfSongsInDvd));

    * **InitCurrentState**
    <br />
    If the current state is maintained in the class that encompasses the state machine or in database, it can be retrieved and initialized using this method before triggering the events. Initial state could also be initialized using the constructor of `FiniteStateMachine`. If the states are of type enum, the first enum will be initialized as the initial state.

        Like this:

            _stateMachine
                .InitCurrentState(MusicPlayerState.Paused)
                .Trigger(MusicPlayerEvent.Stop);

        OR

            new FiniteStateMachine<MusicPlayerState, MusicPlayerEvent>(MusicPlayerState.Playing);

**Note:**

* FiniteStateMachine's `Context` property which is of type `Dictionary<string, object>` could be used to store temporary values. Anything that gets saved in the context stays as long as the FiniteStateMachine object is alive.  
* FiniteStateMachine is stateful, and hence you must instantiate it or the encompassing class on every request.

Sample
--

Here is our sample music player workflow with all the states, events and transitions defined.

    public class MusicPlayerWorkflow
    {
        private FiniteStateMachine<MusicPlayerState, MusicPlayerEvent> _stateMachine;

        public MusicPlayerWorkflow(int numberOfSongsInDvd)
        {
            _numberOfSongsInDvd = numberOfSongsInDvd;
            _stateMachine = new FiniteStateMachine<MusicPlayerState, MusicPlayerEvent>()

            _stateMachine
                .In(MusicPlayerState.Stopped, MusicPlayerState.Paused)
                .On(MusicPlayerEvent.Play)
                .Then(MusicPlayerState.Playing);

            _stateMachine
                .In(MusicPlayerState.Playing, MusicPlayerState.Paused)
                .On(MusicPlayerEvent.Stop)
                .Then(MusicPlayerState.Stopped);

            _stateMachine
                .In(MusicPlayerState.Stopped, MusicPlayerState.Paused, MusicPlayerState.Playing)
                .On(MusicPlayerEvent.Eject)
                .Then(MusicPlayerState.Stopped);
        }

        public MusicPlayerState Play()
		{
			var nextState = _stateMachine.Trigger(MusicPlayerEvent.Play);
			return nextState;
		}

        public MusicPlayerState Stop()
		{
			var nextState = _stateMachine
                                .InitCurrentState(MusicPlayerState.Playing)
                                .Trigger(MusicPlayerEvent.Stop);
			return nextState;
		}
    }


Here is the **state diagram** of our sample state machine:

![Music Player](assets/musicplayer.svg)



