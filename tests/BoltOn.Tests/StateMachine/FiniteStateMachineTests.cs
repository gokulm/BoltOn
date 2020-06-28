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
		public void Trigger_AvailableEvent_ReturnsExpectedState()
		{
			// arrange
			var sut = new FiniteStateMachine<MusicPlayerState, MusicPlayerEvent>(MusicPlayerState.Stopped);
			sut.In(MusicPlayerState.Stopped, MusicPlayerState.Paused)
					.On(MusicPlayerEvent.Play)
					.Then(MusicPlayerState.Playing);

			// act
			var nextState = sut.Trigger(MusicPlayerEvent.Play);

			// assert
			Assert.Equal(MusicPlayerState.Playing, nextState);
		}

		[Fact]
		public void Trigger_UnavailableEvent_ThrowsException()
		{
			// arrange
			var sut = new FiniteStateMachine<MusicPlayerState, MusicPlayerEvent>(MusicPlayerState.Stopped);
			sut.In(MusicPlayerState.Stopped, MusicPlayerState.Paused)
					.On(MusicPlayerEvent.Play)
					.Then(MusicPlayerState.Playing);

			// act
			var result = Record.Exception(() => sut.Trigger(MusicPlayerEvent.Stop));

			// assert
			Assert.NotNull(result);
			Assert.Equal("Current State: Stopped. Event not allowed: Stop", result.Message);
		}

		[Fact]
		public void Trigger_InitializedCurrentStateNotDeclared_ThrowsException()
		{
			// arrange
			var sut = new FiniteStateMachine<MusicPlayerState, MusicPlayerEvent>(MusicPlayerState.Stopped);

			// act
			var result = Record.Exception(() => sut.Trigger(MusicPlayerEvent.Stop));

			// assert
			Assert.NotNull(result);
			Assert.Equal("State not found: Stopped", result.Message);
		}

		[Fact]
		public void Trigger_CurrentStateNotInitialized_TakesFirstStateAsDefaultForStructs()
		{
			// arrange
			var sut = new FiniteStateMachine<MusicPlayerState, MusicPlayerEvent>();
			sut.In(MusicPlayerState.Stopped, MusicPlayerState.Paused)
					.On(MusicPlayerEvent.Play)
					.Then(MusicPlayerState.Playing);

			// act
			var nextState = sut.Trigger(MusicPlayerEvent.Play);

			// assert
			Assert.Equal(MusicPlayerState.Playing, nextState);
		}

		[Fact]
		public void Trigger_CurrentStateSetUsingSetCurrentState_ReturnsExpectedState()
		{
			// arrange
			var sut = new FiniteStateMachine<MusicPlayerState, MusicPlayerEvent>();
			sut.SetCurrentState(MusicPlayerState.Playing);
			sut.In(MusicPlayerState.Playing, MusicPlayerState.Paused)
					.On(MusicPlayerEvent.Stop)
					.Then(MusicPlayerState.Stopped);

			// act
			var nextState = sut.Trigger(MusicPlayerEvent.Stop);

			// assert
			Assert.Equal(MusicPlayerState.Stopped, nextState);
		}
	}
}
