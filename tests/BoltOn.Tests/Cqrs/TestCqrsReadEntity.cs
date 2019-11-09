using BoltOn.Cqrs;

namespace BoltOn.Tests.Cqrs
{
    public class TestCqrsReadEntity : BaseCqrsEntity
    {
        public virtual string Input1 { get; internal set; }

        public virtual string Input2Property1 { get; internal set; }

        public virtual int Input2Property2 { get; internal set; }

        public bool UpdateInput(TestCqrsUpdatedEvent @event)
        {
            return ProcessEvent(@event, e =>
            {
                Input1 = e.Input1;
                Input2Property1 = e.Input2.Property1;
                Input2Property2 = e.Input2.Property2;
            });
        }
    }
}
