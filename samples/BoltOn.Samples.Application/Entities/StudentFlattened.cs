using System;
using BoltOn.Cqrs;
using BoltOn.Samples.Application.Handlers;
using Newtonsoft.Json;

namespace BoltOn.Samples.Application.Entities
{
	public class StudentFlattened : BaseCqrsEntity
    {
		[JsonProperty("id")]
		public override Guid Id { get; set; }

		[JsonProperty("firstName")]
		public string FirstName { get; private set; }

		[JsonProperty("lastName")]
		public string LastName { get; private set; }

		[JsonProperty("studentType")]
		public string StudentType { get; private set; }

		[JsonProperty("studentTypeId")]
		public int StudentTypeId { get; private set; }

		private StudentFlattened()
        {
        }

        internal StudentFlattened(StudentCreatedEvent @event)
        {
            ProcessEvent(@event, e =>
            {
                Id = e.StudentId;
                FirstName = e.FirstName;
                LastName = e.LastName;
				StudentType = e.StudentType;
				StudentTypeId = e.StudentTypeId;
            });
        }

		public void Update(StudentUpdatedEvent @event)
		{
			ProcessEvent(@event, e =>
			{
				Id = e.StudentId;
				FirstName = e.FirstName;
				LastName = e.LastName;
				StudentType = e.StudentType;
				StudentTypeId = e.StudentTypeId;
			});
		}
	}
}
