using System.Linq;
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
            var efAutoDetectChangesMiddleware = new Mock<IBoltOnLogger<EFQueryTrackingBehaviorMiddleware>>();
            efAutoDetectChangesMiddleware.Setup(s => s.Debug(It.IsAny<string>()))
                                     .Callback<string>(st => MediatorTestHelper.LoggerStatements.Add(st));
            context.Container.AddTransient((s) => efAutoDetectChangesMiddleware.Object);
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
			//var student = _studentRepository.Add(new Student { Id = request.StudentId, FirstName = "aa", LastName = "bb"});
			//var student = _studentRepository.FindBy(f => f.Id == request.StudentId).First();
			var student = _studentRepository.GetById(request.StudentId);
			student.FirstName = "aaaa";
			_studentRepository.Update(student);
			return student;
		}
	}
}
