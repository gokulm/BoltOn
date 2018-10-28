using System;
namespace BoltOn.Utilities
{
	public interface ICurrentDateTimeRetriever
	{
		DateTime Get();
	}

    public class CurrentDateTimeRetriever : ICurrentDateTimeRetriever
    {
        public DateTime Get()
        {
            return DateTime.Now;
        }
    }
}
