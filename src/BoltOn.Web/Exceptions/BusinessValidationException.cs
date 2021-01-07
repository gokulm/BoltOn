using System;

namespace BoltOn.Web.Exceptions
{
	public class BusinessValidationException : Exception
	{
		public BusinessValidationException(string message) : base(message)
		{
		}

		public BusinessValidationException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
