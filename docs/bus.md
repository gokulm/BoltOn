BoltOn uses [MassTransit](https://masstransit-project.com/) bus for all the queueing. 

In order to use the bus, do the following:

* Install **BoltOn.Bus.MassTransit** NuGet package.
* Call `BoltOnMassTransitModule()` in your startup's BoltOn() method. 
* For all the applications that will be just publishing to the queue, configure RabbitMq host and all other settings using MassTransit's extension method for `AddMassTransit`. Check out [this page](https://masstransit-project.com/MassTransit/usage/containers/msdi.html) for the supported configuration. Also refer MassTransit's documentation for all the other supported transports (other than RabbitMq), BoltOnMassTransitModule is transport agnostic.
* For all the applications that will be consuming messages from the queue, follow all the above steps and then configure BoltOn's `AppMessageConsumer<TMessage>` provided by the above mentioned NuGet package.
* Finally, inject `IAppServiceBus` in your application and call `PublishAsync` method to publish your message. 

Example:

**Publisher Configuration**

    services.BoltOn(options =>
    {
        options.BoltOnMassTransitBusModule();
    });

    services.AddMassTransit(x =>
    {
        x.AddBus(provider => MassTransit.Bus.Factory.CreateUsingRabbitMq(cfg =>
        {
            var host = cfg.Host(new Uri("rabbitmq://localhost:5672"), hostConfigurator =>
            {
                hostConfigurator.Username("guest");
                hostConfigurator.Password("guest");
            });
        }));
    });

**Consumer Configuration**

    serviceCollection.AddMassTransit(x =>
    {
        x.AddConsumer<AppMessageConsumer<StudentCreatedEvent>>()
            .Endpoint(e =>
            {
                e.Name = $"{nameof(StudentCreatedEvent)}_queue";
            });

        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(new Uri(rabbitmqUri), hostConfigurator =>
            {
                hostConfigurator.Username("guest");
                hostConfigurator.Password("guest");
            });

            cfg.ReceiveEndpoint($"{nameof(StudentCreatedEvent)}_queue", e =>
            {
                e.ConfigureConsumer<AppMessageConsumer<StudentCreatedEvent>>(context);
            });
        });
    });

**Note:**

* As MassTransit had abstracted out the transport like RabbitMq, Azure Service Bus etc., and all the other things very well, BoltOn just adds a minor add-on `AppMessageConsumer<TMessage>` to it, which injects `IRequestor` for processing `TMessage` of type [`IRequest`](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn/Requestor/Pipeline/IRequest.cs). 
* As the consumer injects `IRequestor` and uses it for processing the messages,
please refer to [Requestor](../requestor) documentation to know how to add handlers and its internals.
* Starting and stopping bus gets taken care by [PostRegistrationTask](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn.Bus.MassTransit/PostRegistrationTask.cs) and [CleanupTask](https://github.com/gokulm/BoltOn/blob/master/src/BoltOn.Bus.MassTransit/CleanupTask.cs) respectively. 
