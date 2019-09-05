BoltOn uses [MassTransit](https://masstransit-project.com/) bus for all the queueing. 

In order to use the bus with RabbitMq as the underlying transport, do the following:

* Install **BoltOn.Bus.MassTransit** NuGet package.
* Call `BoltOnMassTransitModule()` in your startup's BoltOn() method. 
* For all the applications that will be just publishing to the queue, configure RabbitMq host and all other settings using MassTransit's extension method for `AddMassTransit`. Refer [this link](https://masstransit-project.com/MassTransit/usage/containers/msdi.html). Refer MassTransit's documentation for all the other transports too.
* For all the applications that will be consuming messages from the queue, follow all the above steps and then configure BoltOn's `BoltOnMassTransitConsumer<TMessage>` provided by the above mentioned NuGet package. 
* Finally, inject `IBus` anywhere in your application and call `PublishAsync` method to publish your message. 

Example:

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

            cfg.ReceiveEndpoint(host, "CreateStudent_Queue", endpoint =>
            {
                endpoint.Consumer(() => provider.GetService<BoltOnMassTransitConsumer<CreateStudent>>());
            });
        }));
    });

You could add an extension method for your transport something like the one mentioned below to configure consumers:

    public static void BoltOnConsumer<TRequest>(this IRabbitMqBusFactoryConfigurator configurator, 
        IServiceProvider serviceProvider, IRabbitMqHost host, string queueName = null)
            where TRequest : class, IRequest
    {
        configurator.ReceiveEndpoint(host, queueName ?? $"{typeof(TRequest).Name}_Queue", endpoint =>
        {
            endpoint.Consumer(() => serviceProvider.GetService<BoltOnMassTransitConsumer<TRequest>>());
        });
    } 

and then call `BoltOnConsumer<CreateStudent>(provider, host)`

**Note:**

* As MassTransit had abstracted out the transport like RabbitMq, Azure Service Bus etc., and all the other things very well BoltOn just adds a minor add-on `BoltOnMassTransitConsumer<TMessage>` to it, which injects `IMediator` for processing the message of type `TMessage`.
* As the consumer injects `IMediator` and uses it for processing the messages, all the messages should implement any of the interfaces mentioned [here](../mediator/#request-response-and-requesthandler). 
Please refer to [Mediator](../mediator) documentation to know how to add handlers and its internals.
* Starting and stopping bus gets taken care by PostRegistrationTask and CleanupTask respectively. 