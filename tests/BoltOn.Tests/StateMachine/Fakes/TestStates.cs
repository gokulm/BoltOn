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
}
