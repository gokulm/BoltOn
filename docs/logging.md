Logging
-------
BoltOn uses .NET Core's logger internally, with just a custom adapter to mainly support unit testing, as .NET Core's ILogger has only one method and all the other methods are extension methods. You could use any logging provider as you wish, or you could inherit [`AppLogger<TType>`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Logging/AppLogger.cs) and override the logging methods. Also, [`AppLoggerFactory`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Logging/AppLoggerFactory.cs) can be used when `AppLogger` cannot be used.

Serilog
-------
In order to use Serilog, you need to do the following:

* Install **BoltOn.Logging.Serilog** NuGet package.
* Call `BoltOnSerilogModule()` in your startup's BoltOn() method, and pass `IConfiguration` object, which basically registers `IAppLogger` to [`AppSerilogLogger`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn.Logging.Serilog/AppSerilogLogger.cs)

[`LoggerContext`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Logging/LoggerContext.cs) is a scoped object, which can be injected anywhere and all the attributes that need to be logged can be added to it, which is what [`RequestLoggerContextMiddleware`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn.Web/Middlewares/RequestLoggerContextMiddleware.cs) does.

Check out Serilog's documentation to know more about sinks and they can be configured in code/appSettings.