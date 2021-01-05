using System.Collections.Generic;

namespace BoltOn.Context
{
	public abstract class BaseContext
    {
        private readonly Dictionary<string, object> _keyValues = new Dictionary<string, object>();

        public virtual Dictionary<string, object>.KeyCollection Keys => _keyValues.Keys;

        public virtual TValue GetByKey<TValue>(string key, TValue defaultValue = default)
        {
            if (_keyValues.ContainsKey(key))
                return (TValue)_keyValues[key];
            return defaultValue;
        }

        public virtual void SetByKey<TValue>(string key, TValue value)
        {
            _keyValues[key] = value;
        }

        public virtual TValue Get<TValue>(TValue defaultValue = default) where TValue : class
        {
            var key = typeof(TValue).Name;
            if (_keyValues.ContainsKey(key))
                return (TValue)_keyValues[key];
            return defaultValue;
        }

        public virtual void Set<TValue>(TValue value)
        {
            var key = typeof(TValue).Name;
            _keyValues[key] = value;
        }
    }
}
