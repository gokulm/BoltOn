using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Bus;
using BoltOn.Mediator.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using BoltOn.Bus.RabbitMq;
using BoltOn.Logging;
using Moq;
using BoltOn.Tests.Other;
using System.Linq;
using BoltOn.Bootstrapping;
using MassTransit;

namespace BoltOn.Tests.Bus
{
	[Collection("IntegrationTests")]
	public class MassTransitBoltOnBusIntegrationTests : IDisposable
	{
		public MassTransitBoltOnBusIntegrationTests()
		{
			Bootstrapper
				.Instance
				.Dispose();
		}

		[Fact]
		public async Task Publish_Message_GetsConsumed()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(b =>
			{
				b.BoltOnAssemblies(GetType().Assembly);
				b.BoltOnRabbitMqBusModule();
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

					cfg.BoltOnConsumer<CreateTestStudent>(host);
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
			Bootstrapper
				.Instance
				.Dispose();
		}
	}

	public class CreateTestStudent : IRequest
	{
		public string FirstName { get; set; }
		public Guid CorrelationId { get; set; } = Guid.NewGuid();
	}


	public class CreateTestStudentHandler : IRequestAsyncHandler<CreateTestStudent>
	{
		private readonly IBoltOnLogger<CreateTestStudentHandler> _logger;

		public CreateTestStudentHandler(IBoltOnLogger<CreateTestStudentHandler> logger)
		{
			this._logger = logger;
		}

		public async Task HandleAsync(CreateTestStudent request, CancellationToken cancellationToken)
		{
			_logger.Debug($"{nameof(CreateTestStudentHandler)} invoked");
			await Task.FromResult("testing");
		}
	}
}
