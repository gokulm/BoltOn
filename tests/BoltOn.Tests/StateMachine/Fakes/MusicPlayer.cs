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
		Next,
		Eject
	}

	public class MusicPlayerWorkflow
	{
		private FiniteStateMachine<MusicPlayerState, MusicPlayerEvent> _stateMachine;
		private int _numberOfSongsInDvd;

		public MusicPlayerWorkflow(int numberOfSongsInDvd, int currentSongIndex)
		{
			_numberOfSongsInDvd = numberOfSongsInDvd;
			_stateMachine = new FiniteStateMachine<MusicPlayerState, MusicPlayerEvent>(MusicPlayerState.Playing);
			_stateMachine.Context["CurrentSongIndex"] = currentSongIndex;

			_stateMachine.In(MusicPlayerState.Playing)
					.On<int>(MusicPlayerEvent.Next, (c) => c + 1 <= _numberOfSongsInDvd)
					.Then(MusicPlayerState.Playing, () =>
					{
						_stateMachine.Context["CurrentSongIndex"] = (int)_stateMachine.Context["CurrentSongIndex"] + 1;
					})
					.Else(MusicPlayerState.Playing, () => _stateMachine.Context["CurrentSongIndex"] = 1);
		}

		public (MusicPlayerState, int) PlayNext(int inputCurrentSongIndex)
		{
			var nextState = _stateMachine.Trigger(MusicPlayerEvent.Next, inputCurrentSongIndex);
			return (nextState, (int)_stateMachine.Context["CurrentSongIndex"]);
		}
	}
}
