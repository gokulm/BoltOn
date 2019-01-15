using BoltOn.Bootstrapping;
using BoltOn.Logging;
using BoltOn.Mediator.Data.EF;
using BoltOn.Mediator.Pipeline;
using BoltOn.Tests.Data.EF;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace BoltOn.Tests.Mediator.Data.EF
{
	public class TestMediatorDataRegistrationTask : IBootstrapperRegistrationTask
    {
        public void Run(RegistrationTaskContext context)
        {
            var efAutoDetectChangesMiddleware = new Mock<IBoltOnLogger<EFAutoDetectChangesDisablingMiddleware>>();
            efAutoDetectChangesMiddleware.Setup(s => s.Debug(It.IsAny<string>()))
                                     .Callback<string>(st => MediatorTestHelper.LoggerStatements.Add(st));
            context.Container.AddTransient((s) => efAutoDetectChangesMiddleware.Object);
        }
    }

	public class TestDataEFQuery : IQuery<Student>
	{
		public int StudentId { get; set; }
	}

	public class TestDataEFHandler : IRequestHandler<TestDataEFQuery, Student>
	{
		readonly IStudentRepository _studentRepository;

		public TestDataEFHandler(IStudentRepository studentRepository)
		{
			this._studentRepository = studentRepository;
		}

		public virtual Student Handle(TestDataEFQuery request)
		{
			var student = _studentRepository.GetById(request.StudentId);
			return student;
		}
	}
}
