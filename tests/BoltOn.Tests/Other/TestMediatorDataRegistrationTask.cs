using System.Linq;
using BoltOn.Bootstrapping;
using BoltOn.Logging;
using BoltOn.Mediator.Data.EF;
using BoltOn.Mediator.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace BoltOn.Tests.Other
{
	public class TestMediatorDataRegistrationTask : IBootstrapperRegistrationTask
    {
        public void Run(RegistrationTaskContext context)
        {
            var efAutoDetectChangesInterceptor = new Mock<IBoltOnLogger<EFQueryTrackingBehaviorInterceptor>>();
            efAutoDetectChangesInterceptor.Setup(s => s.Debug(It.IsAny<string>()))
                                     .Callback<string>(st => MediatorTestHelper.LoggerStatements.Add(st));
            context.Container.AddTransient((s) => efAutoDetectChangesInterceptor.Object);
        }
    }

	public class GetStudent : IQuery<Student>
	{
		public int StudentId { get; set; }
	}

	public class GetStudentHandler : IRequestHandler<GetStudent, Student>
	{
		readonly IStudentRepository _studentRepository;

		public GetStudentHandler(IStudentRepository studentRepository)
		{
			this._studentRepository = studentRepository;
		}

		public virtual Student Handle(GetStudent request)
		{
			var student = _studentRepository.FindBy(f => f.Id == request.StudentId).FirstOrDefault();
			return student;
		}
	}
}
