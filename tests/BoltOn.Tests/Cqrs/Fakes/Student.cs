using System;
using BoltOn.Cqrs;

namespace BoltOn.Tests.Cqrs.Fakes
{
	public class Student : BaseCqrsEntity
	{
		public string Name { get; internal set; }

		public Student()
		{
		}

		public Student(string name, Guid? id = null)
		{
			Name = name;

			if (!id.HasValue)
				Id = Guid.NewGuid();
			else
				Id = id.GetValueOrDefault();

			RaiseEvent(new StudentCreatedEvent
			{
				Input = name
			});
		}

		public void Modify(UpdateStudentRequest request)
		{
			Name = request.Input;
			RaiseEvent(new StudentUpdatedEvent
			{
				Id = CqrsConstants.EventId,
				Name = request.Input,
				Input2 = new TestInput { Property1 = "prop1", Property2 = 10 }
			});

			RaiseEvent(new TestCqrsUpdated2Event
			{
				Id = CqrsConstants.Event2Id,
				Input1 = request.Input,
				Input2 = new TestInput { Property1 = "prop2", Property2 = 20 }
			});
		}
	}

	public class TestInput
	{
		public string Property1 { get; set; }

		public int Property2 { get; set; }
	}
}
