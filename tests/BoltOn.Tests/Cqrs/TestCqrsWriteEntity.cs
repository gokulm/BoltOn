using System;
using BoltOn.Cqrs;

namespace BoltOn.Tests.Cqrs
{
    public class TestCqrsWriteEntity : BaseCqrsEntity
    {
        public string Input { get; internal set; }

		public TestCqrsWriteEntity()
		{
		}

		public TestCqrsWriteEntity(string input)
		{
			Input = input;
			Id = Guid.NewGuid();
			RaiseEvent(new TestCqrsCreatedEvent
			{
				Input = input
			}); 
		}

        public void ChangeInput(TestCqrsRequest request)
        {
            Input = request.Input;
            RaiseEvent(new TestCqrsUpdatedEvent
            {
                Id = CqrsConstants.EventId,
                Input1 = request.Input,
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
