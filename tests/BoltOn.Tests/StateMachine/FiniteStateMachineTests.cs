using BoltOn.StateMachine;
using BoltOn.Tests.StateMachine.Fakes;
using Xunit;

namespace BoltOn.Tests.StateMachine
{
	public class FiniteStateMachineTests
	{
		[Theory]
		[InlineData(MusicPlayerState.Stopped)]
		[InlineData(MusicPlayerState.Paused)]
		public void Trigger_AvailableEvent_ReturnsExpectedState(MusicPlayerState currentState)
		{
			// arrange
			var sut = new FiniteStateMachine<MusicPlayerState, MusicPlayerEvent>(currentState);
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
		public void Trigger_InitializeCurrentState_ReturnsExpectedState()
		{
			// arrange
			var sut = new FiniteStateMachine<MusicPlayerState, MusicPlayerEvent>();
			sut.InitCurrentState(MusicPlayerState.Playing);
			sut.In(MusicPlayerState.Playing, MusicPlayerState.Paused)
					.On(MusicPlayerEvent.Stop)
					.Then(MusicPlayerState.Stopped);

			// act
			var nextState = sut.Trigger(MusicPlayerEvent.Stop);
			var nextState2 = sut
					.InitCurrentState(MusicPlayerState.Paused)
					.Trigger(MusicPlayerEvent.Stop);

			// assert
			Assert.Equal(MusicPlayerState.Stopped, nextState);
			Assert.Equal(MusicPlayerState.Stopped, nextState2);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void Trigger_OnEventWithCondition_ReturnsExpectedState(bool isDvdInserted)
		{
			// arrange
			var sut = new FiniteStateMachine<MusicPlayerState, MusicPlayerEvent>();

			sut.In(MusicPlayerState.Stopped)
					.On(MusicPlayerEvent.Play, () => isDvdInserted)
					.Then(MusicPlayerState.Playing)
					.Else(MusicPlayerState.Stopped);

			// act
			var nextState = sut.Trigger(MusicPlayerEvent.Play);

			// assert
			if (isDvdInserted)
				Assert.Equal(MusicPlayerState.Playing, nextState);
			else
				Assert.Equal(MusicPlayerState.Stopped, nextState);
		}

		[Fact]
		public void Trigger_TriggerChainOfEvents_ReturnsExpectedState()
		{
			// arrange
			var sut = new FiniteStateMachine<MusicPlayerState, MusicPlayerEvent>();
			var isDvdInserted = true;

			sut.In(MusicPlayerState.Stopped)
				  .On(MusicPlayerEvent.Play, () => isDvdInserted)
				  .Then(MusicPlayerState.Playing)
				  .Else(MusicPlayerState.Stopped);
			sut.In(MusicPlayerState.Playing, MusicPlayerState.Stopped)
					.On(MusicPlayerEvent.Eject)
					.Then(MusicPlayerState.Stopped, () => isDvdInserted = false);

			// act
			var nextState1 = sut.Trigger(MusicPlayerEvent.Play);
			var nextState2 = sut.Trigger(MusicPlayerEvent.Eject);
			var nextState3 = sut.Trigger(MusicPlayerEvent.Play);

			var test = sut.GetDotData();

			// assert
			Assert.Equal(MusicPlayerState.Playing, nextState1);
			Assert.Equal(MusicPlayerState.Stopped, nextState2);
			Assert.Equal(MusicPlayerState.Stopped, nextState3);
		}

		[Theory]
		[InlineData(10)]
		[InlineData(8)]
		public void Trigger_ThenAction_ReturnsExpectedState(int currentSongIndex)
		{
			// arrange
			var sut = new FiniteStateMachine<MusicPlayerState, MusicPlayerEvent>(MusicPlayerState.Playing);
			var numberOfSongsInDvd = 10;
			var inputCurrentSongIndex = currentSongIndex;

			sut.In(MusicPlayerState.Playing)
					.On(MusicPlayerEvent.Next)
					.Then(MusicPlayerState.Playing, () => PlayNextSong());

			// act
			var nextState = sut.Trigger(MusicPlayerEvent.Next);

			// assert
			Assert.Equal(MusicPlayerState.Playing, nextState);
			if (inputCurrentSongIndex == numberOfSongsInDvd)
				Assert.Equal(10, currentSongIndex);
			else
				Assert.Equal(9, currentSongIndex);


			void PlayNextSong()
			{
				if (currentSongIndex + 1 > numberOfSongsInDvd)
					currentSongIndex = numberOfSongsInDvd;
				else
					currentSongIndex += 1;
			}
		}

		[Theory]
		[InlineData(8)]
		[InlineData(10)]
		public void Trigger_OnEventWithConditionThenActionAndElseAction_ReturnsExpectedState(int currentSongIndex)
		{
			// arrange
			var inputCurrentSongIndex = currentSongIndex;
			var sut = new FiniteStateMachine<MusicPlayerState, MusicPlayerEvent>(MusicPlayerState.Playing);
			var numberOfSongsInDvd = 10;

			sut.In(MusicPlayerState.Playing)
					.On(MusicPlayerEvent.Next, () => currentSongIndex + 1 <= numberOfSongsInDvd)
					.Then(MusicPlayerState.Playing, () => currentSongIndex += 1)
					.Else(MusicPlayerState.Playing, () => currentSongIndex = 1);

			// act
			var nextState = sut.Trigger(MusicPlayerEvent.Next);

			// assert
			Assert.Equal(MusicPlayerState.Playing, nextState);
			if (inputCurrentSongIndex == numberOfSongsInDvd)
				Assert.Equal(1, currentSongIndex);
			else
				Assert.Equal(9, currentSongIndex);
		}
	}
}
