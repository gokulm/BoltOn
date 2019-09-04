* **Check.Requires**
<br>
There are instances where you have to check for a condition and throw exception if the condition fails, in those instances you could use `Check.Requires`

    Example:

        Check.Requires(_serviceCollection != null, "ServiceCollection not initialized"); 

    is equivalent to

        if(_serviceCollection == null)
            throw new Exception("ServiceCollection not initialized");

    and custom exceptions can be thrown like this:

        Check.Requires<CustomException>(_serviceCollection != null, "ServiceCollection not initialized"); 

* **IBoltOnClock/BoltOnClock**
<br>
There are instances where you have to use static properties DateTime.Now or DateTimeOffset.UtcNow, which makes hard to unit test, in those instances you could inject `IBoltOnClock`