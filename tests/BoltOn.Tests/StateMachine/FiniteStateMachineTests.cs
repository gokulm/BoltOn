using System;
using BoltOn.StateMachine;
using BoltOn.Tests.StateMachine.Fakes;
using Xunit;

namespace BoltOn.Tests.StateMachine
{
	public class FiniteStateMachineTests
	{
		private readonly IFiniteStateMachine<MusicPlayerState, MusicPlayerEvent> _sut;

		public FiniteStateMachineTests()
		{
			_sut = new FiniteStateMachine<MusicPlayerState, MusicPlayerEvent>(MusicPlayerState.Stopped);

			_sut.In(MusicPlayerState.Stopped, MusicPlayerState.Paused)
					.On(MusicPlayerEvent.Play)
					.Then(MusicPlayerState.Playing);

			_sut.In(MusicPlayerState.Playing, MusicPlayerState.Paused)
					.On(MusicPlayerEvent.Stop)
					.Then(MusicPlayerState.Stopped);
		}

		[Fact]
		public void Test()
		{
			// act
			var nextState = _sut.Trigger(MusicPlayerEvent.Play);

			// assert
			Assert.Equal(MusicPlayerState.Playing, nextState);
		}
	}
}
