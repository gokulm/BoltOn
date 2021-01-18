using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using BoltOn.Bus.MassTransit;
using BoltOn.Logging;
using Moq;
using BoltOn.Tests.Other;
using System.Linq;
using BoltOn.Tests.Requestor.Fakes;
using MassTransit;
using BoltOn.Tests.Bus.Fakes;

namespace BoltOn.Tests.Bus
{
    [Collection("IntegrationTests")]
    public class AppServiceBusIntegrationTests : IDisposable
    {
        [Fact]
        public async Task PublishAsync_PublishToInMemoryHost_GetsConsumed()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.BoltOn(b =>
            {
                b.BoltOnAssemblies(GetType().Assembly);
                b.BoltOnMassTransitBusModule();
            });

            serviceCollection.AddMassTransit(x =>
            {
                x.AddBus(provider => MassTransit.Bus.Factory.CreateUsingInMemory(cfg =>
                {
                    cfg.ReceiveEndpoint("CreateTestStudent_queue", ep =>
                    {
                        ep.Consumer(() => provider.GetService<AppMessageConsumer<CreateTestStudent>>());
                    });
                }));
            });


            var logger = new Mock<IAppLogger<CreateTestStudentHandler>>();
            logger.Setup(s => s.Debug(It.IsAny<string>()))
                                .Callback<string>(st => RequestorTestHelper.LoggerStatements.Add(st));
            serviceCollection.AddTransient((s) => logger.Object);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            serviceProvider.TightenBolts();
            var bus = serviceProvider.GetService<BoltOn.Bus.IAppServiceBus>();

            // act
            await bus.PublishAsync(new CreateTestStudent { FirstName = "test" });
            // as assert not working after async method, added sleep
            Thread.Sleep(1000);

            // assert
            var result = RequestorTestHelper.LoggerStatements.FirstOrDefault(f => f ==
                                        $"{nameof(CreateTestStudentHandler)} invoked");
            Assert.NotNull(result);
        }

        [Fact]
        public async Task PublishAsync_PublishToRabbitMq_GetsConsumed()
        {
            if (!IntegrationTestHelper.IsRabbitMqRunning)
                return;

            var serviceCollection = new ServiceCollection();
            serviceCollection.BoltOn(b =>
            {
                b.BoltOnAssemblies(GetType().Assembly);
                b.BoltOnMassTransitBusModule();
            });

            serviceCollection.AddMassTransit(x =>
            {
                x.AddBus(provider => MassTransit.Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    cfg.Host(new Uri("rabbitmq://localhost:5010"), hostConfigurator =>
                    {
                        hostConfigurator.Username("guest");
                        hostConfigurator.Password("guest");
                    });

                    cfg.ReceiveEndpoint($"{nameof(CreateTestStudent)}_Queue", endpoint =>
                    {
                        endpoint.Consumer(provider.GetService<AppMessageConsumer<CreateTestStudent>>);
                    });
                }));
            });

            var logger = new Mock<IAppLogger<CreateTestStudentHandler>>();
            logger.Setup(s => s.Debug(It.IsAny<string>()))
                                .Callback<string>(st => RequestorTestHelper.LoggerStatements.Add(st));
            serviceCollection.AddTransient((s) => logger.Object);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            serviceProvider.TightenBolts();
            var bus = serviceProvider.GetService<BoltOn.Bus.IAppServiceBus>();

            // act
            await bus.PublishAsync(new CreateTestStudent { FirstName = "test" });
            // as assert not working after async method, added sleep
            Thread.Sleep(1000);

            // assert
            var result = RequestorTestHelper.LoggerStatements.FirstOrDefault(f => f ==
                                        $"{nameof(CreateTestStudentHandler)} invoked");
            Assert.NotNull(result);
        }

        [Fact]
        public async Task PublishAsync_PublishToAzureServiceBus_GetsConsumed()
        {
            if (!IntegrationTestHelper.IsAzureServiceBusRunning)
                return;

            var serviceCollection = new ServiceCollection();
            serviceCollection.BoltOn(b =>
            {
                b.BoltOnAssemblies(GetType().Assembly);
                b.BoltOnMassTransitBusModule();
            });

            serviceCollection.AddMassTransit(x =>
            {
                x.UsingAzureServiceBus((context, cfg) =>
                {
                    cfg.Host("ADD_CONNECTIONSTRING");

                    cfg.ReceiveEndpoint($"createteststudent_queue", endpoint =>
                    {
                        endpoint.Consumer(context.GetService<AppMessageConsumer<CreateTestStudent>>);
                    });
                });
            });

            var logger = new Mock<IAppLogger<CreateTestStudentHandler>>();
            logger.Setup(s => s.Debug(It.IsAny<string>()))
                                .Callback<string>(st => RequestorTestHelper.LoggerStatements.Add(st));
            serviceCollection.AddTransient((s) => logger.Object);

            var serviceProvider = serviceCollection.BuildServiceProvider();
            serviceProvider.TightenBolts();
            var bus = serviceProvider.GetService<BoltOn.Bus.IAppServiceBus>();

            // act
            await bus.PublishAsync(new CreateTestStudent { FirstName = "test" });
            // as assert not working after async method, added sleep
            Thread.Sleep(2000);

            // assert
            var result = RequestorTestHelper.LoggerStatements.FirstOrDefault(f => f ==
                                        $"{nameof(CreateTestStudentHandler)} invoked");
            Assert.NotNull(result);
        }

        public void Dispose()
        {
            RequestorTestHelper.LoggerStatements.Clear();
        }
    }
}
