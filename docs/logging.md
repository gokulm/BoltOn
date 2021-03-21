Logging
-------
BoltOn uses .NET Core's logger internally, with just a custom adapter to mainly support unit testing, as .NET Core's ILogger has only one method and all the other methods are extension methods. You could use any logging provider as you wish, or you could inherit [`AppLogger<TType>`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Logging/AppLogger.cs) and override the logging methods. Also, [`AppLoggerFactory`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Logging/AppLoggerFactory.cs) can be used when `AppLogger` cannot be used.

Serilog
-------
As `IAppLogger` is just an adapter to .NET Core's ILogger, Serilog or any other logging provider can be used in your applications. 

Check out Serilog's documentation to know more about sinks and how they can be configured in code/appSettings.