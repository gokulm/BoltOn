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
using MassTransit;
using BoltOn.Tests.Mediator.Fakes;

namespace BoltOn.Tests.Bus
{
    [Collection("IntegrationTests")]
	public class BoltOnMassTransitBusIntegrationTests : IDisposable
	{	 
		[Fact]
		public async Task PublishAsync_InMemoryHost_GetsConsumed()
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
						ep.Consumer(() => provider.GetService<BoltOnMassTransitConsumer<CreateTestStudent>>());
					});
				}));
			});


			var logger = new Mock<IBoltOnLogger<CreateTestStudentHandler>>();
			logger.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => MediatorTestHelper.LoggerStatements.Add(st));
			serviceCollection.AddTransient((s) => logger.Object);

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var bus = serviceProvider.GetService<BoltOn.Bus.IBus>();

			// act
			await bus.PublishAsync(new CreateTestStudent { FirstName = "test" });
			// as assert not working after async method, added sleep
			Thread.Sleep(1000);

			// assert
			var result = MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"{nameof(CreateTestStudentHandler)} invoked");
			Assert.NotNull(result);
		}

		[Fact]
		public async Task PublishAsync_Message_GetsConsumed()
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
					var host = cfg.Host(new Uri("rabbitmq://localhost:5672"), hostConfigurator =>
					{
						hostConfigurator.Username("guest");
						hostConfigurator.Password("guest");
					});

					cfg.ReceiveEndpoint("CreateTestStudent_Queue", endpoint =>
					{
						endpoint.Consumer(() => provider.GetService<BoltOnMassTransitConsumer<CreateTestStudent>>());
					});
				}));
			});

			var logger = new Mock<IBoltOnLogger<CreateTestStudentHandler>>();
			logger.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => MediatorTestHelper.LoggerStatements.Add(st));
			serviceCollection.AddTransient((s) => logger.Object);

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var bus = serviceProvider.GetService<BoltOn.Bus.IBus>();

			// act
			await bus.PublishAsync(new CreateTestStudent { FirstName = "test" });
			// as assert not working after async method, added sleep
			Thread.Sleep(1000);

			// assert
			var result = MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"{nameof(CreateTestStudentHandler)} invoked");
			Assert.NotNull(result);
		}

		public void Dispose()
		{
			MediatorTestHelper.LoggerStatements.Clear();
		}
	}
}
