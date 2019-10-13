using BoltOn.Cqrs;
using BoltOn.Samples.Application.Handlers;
using Newtonsoft.Json;

namespace BoltOn.Samples.Application.Entities
{
    public class StudentFlattened : BaseCqrsEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        private StudentFlattened()
        {
        }

        internal StudentFlattened(StudentCreatedEvent @event)
        {
            ProcessEvent(@event, e =>
            {
                var student = JsonConvert.DeserializeObject<Student>(e.Body);
                Id = student.Id;
                FirstName = student.FirstName;
                LastName = student.LastName;
            });
        }
    }
}
