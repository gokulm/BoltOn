BoltOn uses [MassTransit](https://masstransit-project.com/) bus for all the queueing. 

In order to use service bus with RabbitMq as the underlying queueing mechanism, do the following:

1. Install **BoltOn.Bus.RabbitMq** NuGet package.
2. Call `BoltOnRabbitMqBusModule()` in your startup's BoltOn() method. 
3. For all the applications that will be just publishing to the queue, configure RabbitMq host and all other settings using MassTransit's extension method for `AddMassTransit`. Refer [this link](https://masstransit-project.com/MassTransit/usage/containers/msdi.html). 
4. For all the applications that will be consuming messages from the queue, follow all the above steps and then use BoltOn's extension method `BoltOnConsumer<MessageType>` provided by the above mentioned NuGet package. 

**Publisher Configuration**

    services.BoltOn(options =>
    {
        options.BoltOnRabbitMqBusModule();
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

Apart from the publisher configuration, configure the consumer using `BoltOnConsumer` extension method:

    serviceCollection.AddMassTransit(x =>
    {
        x.AddBus(provider => MassTransit.Bus.Factory.CreateUsingRabbitMq(cfg =>
        {
            var host = cfg.Host(new Uri("rabbitmq://localhost:5672"), hostConfigurator =>
            {
                hostConfigurator.Username("guest");
                hostConfigurator.Password("guest");
            });

            cfg.BoltOnConsumer<CreateStudent>(provider, host);
        }));
    });

5. Configure  inject `IBus` and call `PublishAsync` method by passing the object that should be queued. Only objects that implement `IRequest` interface could be published. 