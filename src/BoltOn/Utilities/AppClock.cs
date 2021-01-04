using System;
namespace BoltOn.Utilities
{
	public interface IAppClock
	{
		DateTime Now { get; }
		DateTimeOffset UtcNow { get; }
	}

	public class AppClock : IAppClock
	{
		public DateTime Now => DateTime.Now;
		public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
	}
}
