using System;
namespace BoltOn.Logging
{
    public class BoltOnLoggerDecorator<TType> : IBoltOnLogger<TType>
    {
        readonly IBoltOnLogger<TType> _boltOnLogger;

        public BoltOnLoggerDecorator(IBoltOnLoggerFactory sketchyLoggerFactory)
        {
            this._boltOnLogger = sketchyLoggerFactory.Create<TType>();
        }

        public virtual void Debug(string message)
        {
            _boltOnLogger.Debug(message);
        }

        public virtual void Error(string message)
        {
            _boltOnLogger.Error(message);
        }

        public virtual void Error(Exception exception)
        {
            _boltOnLogger.Error(exception);
        }

        public virtual void Info(string message)
        {
            _boltOnLogger.Info(message);
        }

        public virtual void Warn(string message)
        {
            _boltOnLogger.Warn(message);
        }
    }
}
