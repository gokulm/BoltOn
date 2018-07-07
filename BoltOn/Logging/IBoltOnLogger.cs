using System;
namespace BoltOn.Logging
{
	public interface IBoltOnLogger<TType>
    {
		void Debug(string message);
		void Info(string message);
		void Error(string message);
		void Error(Exception exception);
		void Warn(string message);
    }
}
