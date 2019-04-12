using System;
namespace BoltOn.Utilities
{
	public static class Check
	{
		public static void Requires(bool isTrue, string message)
		{
			if (isTrue) return;

			throw new Exception(message);
		}

		public static void Requires<TException>(bool isTrue, string message) where TException : Exception
		{
			if (isTrue) return;
		    if (Activator.CreateInstance(typeof(TException), message) is TException exception)
				throw exception;
		}
	}
}
