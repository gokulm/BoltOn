[Hangfire](https://www.hangfire.io/) can be used along with BoltOn for all the background and scheduled jobs.

In order to use it, do the following:

* Install **BoltOn.Hangfire** NuGet package.
* Call `BoltOnHangfireModule()` in your startup's BoltOn() method. 
* Configure [Hangfire](https://www.hangfire.io/) by referring to its documentation.
* Once all the configuration is done, create a request (without response) class that implements [`IRequest`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Requestor/Pipeline/IRequest.cs) and a handler that implements [`IHandler<in TRequest>`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Requestor/Pipeline/IHandler.cs). Refer to [Requestor](../requestor/#request-response-and-handler) to know more about the implementation. 
* Finally, use [`AppHangfireJobProcessor`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn.Hangfire/AppHangfireJobProcessor.cs) to configure the request/handler as a background or scheduled job. 

Example:

**While bootstrapping your application**

    serviceCollection.BoltOn(o =>
    {
        o.BoltOnHangfireModule();
    });

    // hangfire configuration
    GlobalConfiguration.Configuration
        .UseSqlServerStorage("connectionstring to the db");

    // a job to be executed every minute 
    RecurringJob.AddOrUpdate<AppHangfireJobProcessor>("StudentsNotifier",
				p => p.ProcessAsync(new NotifyStudentsRequest { JobType = "Recurring" }, default), Cron.Minutely());

    // to schededule an one-time background job  
    BackgroundJob.Schedule<AppHangfireJobProcessor>(p => p.ProcessAsync(new NotifyStudentsRequest { JobType = "OneTime" }, default),
        TimeSpan.FromSeconds(30));

** Request and Handler**

    public class NotifyStudentsRequest : IRequest
    {
		public string JobType { get; set; }

		public override string ToString()
		{
			return "Student Notifier " + JobType;
		}
	}

    public class NotifyStudentsHandler : IHandler<NotifyStudentsRequest>
    {
		private readonly IAppLogger<NotifyStudentsHandler> _logger;

		public NotifyStudentsHandler(IAppLogger<NotifyStudentsHandler> logger)
		{
			_logger = logger;
		}

        public Task HandleAsync(NotifyStudentsRequest request, CancellationToken cancellationToken)
        {
            _logger.Debug($"Notifying students. JobType: {request.JobType}");
            return Task.CompletedTask;
        }
    }

**Note:**

* Please look into [BoltOn.Samples.Console](https://github.com/gokulm/BoltOn/tree/master/samples/BoltOn.Samples.Console) to see the actual code. 
* To know more about RecurringJob or BackgroundJob, please refer to [Hangfire's](https://www.hangfire.io/) documentation.
* Override `ToString()` method of the request object for a descriptive name to be displayed on the Hangfire Dashboard.
* BackgroundJob can be triggered anywhere within your application and need not be while bootstrapping. 