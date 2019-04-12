using System;
namespace BoltOn.Utilities
{
	public interface IBoltOnClock
	{
		DateTime Now { get; }
		DateTimeOffset UtcNow { get; }
	}

    public class BoltOnClock : IBoltOnClock
    {
		public DateTime Now => DateTime.Now;
		public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
