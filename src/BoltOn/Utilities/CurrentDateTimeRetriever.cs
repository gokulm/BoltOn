using System;
namespace BoltOn.Utilities
{
	public interface ICurrentDateTimeRetriever
	{
		DateTime Now { get; }
		DateTimeOffset UtcNow { get; }
	}

    public class CurrentDateTimeRetriever : ICurrentDateTimeRetriever
    {
		public DateTime Now => DateTime.Now;

		public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
