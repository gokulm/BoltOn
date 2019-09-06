Check.Requires
--------------
There are instances where you have to check for a condition and throw exception if the condition fails, in those instances you could use the `Requires` in  [`Check`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Utilities/Check.cs)

Example:

    Check.Requires(_serviceCollection != null, "ServiceCollection not initialized"); 

is equivalent to

    if(_serviceCollection == null)
        throw new Exception("ServiceCollection not initialized");

and custom exceptions can be thrown like this:

    Check.Requires<CustomException>(_serviceCollection != null, "ServiceCollection not initialized"); 

IBoltOnClock/BoltOnClock
------------------------
There are instances where you have to use static properties DateTime.Now or DateTimeOffset.UtcNow, which makes hard to unit test, in those instances you could inject [`IBoltOnClock`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Utilities/BoltOnClock.cs)

BoltOnAppCleaner
----------------
It scans all the `ICleanupTask` in the assembly collection formed by BoltOn() method in the Startup and executes them in the reverse order of assembly dependencies. If your module needs any cleanup work like purging data, stopping bus, disposing objects etc., implement `ICleanupTask` and it will be invoked automatically when internal classes get disposed. In case if you want all the cleanup tasks to be invoked on demand, use the static method `Clean()` in this class. However, make sure that it gets called only once in your application.

In a WebAPI, you could call this method using IApplicationLifetime:

    public void Configure(IApplicationBuilder app, IApplicationLifetime appLifetime)
    {
        app.UseMvc();
        app.ApplicationServices.TightenBolts();
        appLifetime.ApplicationStopping.Register(() => BoltOnAppCleaner.Clean());
    }

BoltOnServiceLocator
--------------------
Service Locator is an [Anti-Pattern](https://blog.ploeh.dk/2010/02/03/ServiceLocatorisanAnti-Pattern/), so try to avoid using it. In case if you want to use it, you could use the Current property in the class which is of type `IServiceProvider`.