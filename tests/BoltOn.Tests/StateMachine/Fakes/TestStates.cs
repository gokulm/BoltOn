using BoltOn.StateMachine;

namespace BoltOn.Tests.StateMachine.Fakes
{
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
		Next
	}

	public class MusicPlayerWorkflow
	{
		private readonly IFiniteStateMachine<MusicPlayerState, MusicPlayerEvent> _finiteStateMachine;

		public MusicPlayerWorkflow()
		{
			_finiteStateMachine = new FiniteStateMachine<MusicPlayerState, MusicPlayerEvent>();

			_finiteStateMachine.In(MusicPlayerState.Stopped, MusicPlayerState.Paused)
					.On(MusicPlayerEvent.Play)
					.Then(MusicPlayerState.Playing);

			_finiteStateMachine.In(MusicPlayerState.Playing, MusicPlayerState.Paused)
					.On(MusicPlayerEvent.Stop)
					.Then(MusicPlayerState.Stopped);
		}
	}
}
