using System;
namespace BoltOn.Logger
{
    public interface IAppLogger<TType>
    {
        void Debug(string message, params object?[] args);
        void Info(string message, params object?[] args);
        void Error(string message, params object?[] args);
        void Error(Exception exception, params object?[] args);
        void Warn(string message, params object?[] args);
    }
}
