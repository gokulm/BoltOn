using System;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Bus;
using BoltOn.Mediator.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using BoltOn.Bus.RabbitMq;
using System.Reflection;
using BoltOn.Logging;
using Moq;
using BoltOn.Tests.Other;
using System.Linq;
using BoltOn.Bootstrapping;

namespace BoltOn.Tests.Bus
{
	[Collection("IntegrationTests")]
	public class MassTransitBoltOnBusIntegrationTests
	{
		public MassTransitBoltOnBusIntegrationTests()
		{
			//Bootstrapper
				//.Instance
				//.Dispose();
		}

		[Fact]
		public async Task Publish_Message_GetsConsumed()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(b =>
			{
				b.BoltOnAssemblies(GetType().Assembly);
			});

			serviceCollection.BoltOnRabbitMqBus(o =>
			{
				o.HostAddress = "rabbitmq://localhost:5672";
				o.Username = "guest";
				o.Password = "guest";
				o.AssembliesWithConsumers.Add(Assembly.GetExecutingAssembly());
			});

			var logger = new Mock<IBoltOnLogger<CreateTestStudentHandler>>();
			logger.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => MediatorTestHelper.LoggerStatements.Add(st));
			serviceCollection.AddTransient((s) => logger.Object);

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			var bus = serviceProvider.GetService<IBus>();

			// act
			await bus.PublishAsync(new CreateTestStudent { FirstName = "test" });

			// assert
			var test = MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == $"{nameof(CreateTestStudentHandler)} invoked");
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f == $"{nameof(CreateTestStudentHandler)} invoked"));
		}
	}

	public class CreateTestStudent : IMessage
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
