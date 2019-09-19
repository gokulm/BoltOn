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
using BoltOn.Bootstrapping;
using MassTransit;
using BoltOn.Mediator.Pipeline;
using BoltOn.Cqrs;
using BoltOn.Data.EF;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using BoltOn.Data;

namespace BoltOn.Tests.Cqrs
{
	[Collection("IntegrationTests")]
	public class CqrsIntegrationTests : IDisposable
	{
		public CqrsIntegrationTests()
		{
			Bootstrapper
				.Instance
				.Dispose();
		}

		[Fact]
		public async Task MediatorHandle_WithCqrs_ReturnsResult()
		{
			var serviceCollection = new ServiceCollection();
			serviceCollection.BoltOn(b =>
			{
				b.BoltOnAssemblies(GetType().Assembly);
				b.BoltOnEFModule();
				b.EnableCqrs();
				b.BoltOnMassTransitBusModule();
			});

			MediatorTestHelper.IsSqlServer = false;

			serviceCollection.AddMassTransit(x =>
			{
				x.AddBus(provider => MassTransit.Bus.Factory.CreateUsingInMemory(cfg =>
				{
					cfg.ReceiveEndpoint("TestCqrsUpdatedEvent_queue", ep =>
					{
						ep.Consumer(() => provider.GetService<BoltOnMassTransitConsumer<TestCqrsUpdatedEvent>>());
					});
				}));
			});


			var logger = new Mock<IBoltOnLogger<TestCqrsHandler>>();
			logger.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => MediatorTestHelper.LoggerStatements.Add(st));
			serviceCollection.AddTransient((s) => logger.Object);

			var logger2 = new Mock<IBoltOnLogger<TestCqrsUpdatedEventHandler>>();
			logger2.Setup(s => s.Debug(It.IsAny<string>()))
								.Callback<string>(st => MediatorTestHelper.LoggerStatements.Add(st));
			serviceCollection.AddTransient((s) => logger2.Object);

			var serviceProvider = serviceCollection.BuildServiceProvider();
			serviceProvider.TightenBolts();
			//var bus = serviceProvider.GetService<BoltOn.Bus.IBus>();
			var mediator = serviceProvider.GetService<IMediator>();

			// act
			await mediator.ProcessAsync(new TestCqrsRequest { Input = "test" });
			// as assert not working after async method, added sleep
			await Task.Delay(1000);
			//Console.ReadLine();

			// assert
			var result = MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"{nameof(TestCqrsHandler)} invoked");
			Assert.NotNull(MediatorTestHelper.LoggerStatements.FirstOrDefault(f => f ==
										$"{nameof(TestCqrsUpdatedEventHandler)} invoked"));
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


	public class TestCqrsRequest : IRequest
	{
		public string Input { get; set; }
	}

	public class TestCqrsHandler : IRequestAsyncHandler<TestCqrsRequest>
	{
		private readonly IBoltOnLogger<TestCqrsHandler> _logger;
		private readonly ITestCqrsEntityRepository _repository;

		public TestCqrsHandler(IBoltOnLogger<TestCqrsHandler> logger,
			ITestCqrsEntityRepository repository)
		{
			_logger = logger;
			_repository = repository;
		}

		public async Task HandleAsync(TestCqrsRequest request, CancellationToken cancellationToken)
		{
			_logger.Debug($"{nameof(TestCqrsHandler)} invoked");
			var testCqrsEntity = new TestCqrsEntity { Id = Guid.NewGuid().ToString() };
			testCqrsEntity.Update(request);
			await _repository.AddAsync(testCqrsEntity);
		}
	}

	public class TestCqrsEntity : BaseCqrsEntity
	{
		public string Input { get; set; }

		public void Update(TestCqrsRequest request)
		{
			Input = request.Input;
			RaiseEvent(new TestCqrsUpdatedEvent { Input = request.Input + " event", SourceId = Id });
		}
	}

	public class TestCqrsUpdatedEvent : BoltOnEvent
	{
		public string Input { get; set; }
	}


	public class TestCqrsUpdatedEventHandler : IRequestAsyncHandler<TestCqrsUpdatedEvent>
	{
		private readonly IBoltOnLogger<TestCqrsUpdatedEventHandler> _logger;

		public TestCqrsUpdatedEventHandler(IBoltOnLogger<TestCqrsUpdatedEventHandler> logger)
		{
			_logger = logger;
		}

		public async Task HandleAsync(TestCqrsUpdatedEvent request, CancellationToken cancellationToken)
		{
			_logger.Debug($"{nameof(TestCqrsUpdatedEventHandler)} invoked");
			await Task.FromResult(1);
		}
	}

	public class TestCqrsEntityMapping : IEntityTypeConfiguration<TestCqrsEntity>
	{
		public void Configure(EntityTypeBuilder<TestCqrsEntity> builder)
		{
			builder
				.ToTable("TestCqrsEntity")
				.HasKey(k => k.Id);
			builder
				.HasMany(p => p.Events);
		}
	}

	public class BoltOnEventMapping : IEntityTypeConfiguration<BoltOnEvent>
	{
		public void Configure(EntityTypeBuilder<BoltOnEvent> builder)
		{
			builder
				.ToTable("BoltOnEvent")
				.HasKey(k => k.Id);
		}
	}

	public interface ITestCqrsEntityRepository : IRepository<TestCqrsEntity>
	{
	}

	public class TestCqrsEntityRepository : BaseEFCqrsRepository<TestCqrsEntity, SchoolDbContext>, ITestCqrsEntityRepository
	{
		public TestCqrsEntityRepository(IDbContextFactory dbContextFactory, EventBag eventBag) : base(dbContextFactory, eventBag)
		{
		}

		//public  void Update2(BaseCqrsEntity entity)
		//{
		//	base.Update(entity as TestCqrsEntity);
		//}
	}

	public class TestCqrsRegistrationTask : IRegistrationTask
	{
		public void Run(RegistrationTaskContext context)
		{
			context.Container.AddTransient<IRepository<TestCqrsEntity>, TestCqrsEntityRepository>();
		}
	}
}