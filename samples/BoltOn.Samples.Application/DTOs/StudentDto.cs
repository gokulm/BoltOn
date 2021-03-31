using System;
using System.Collections.Generic;

namespace BoltOn.Samples.Application.DTOs
{
	public class StudentDto
	{
		public Guid StudentId { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string StudentType { get; set; }
		public string Email { get; set; }
		public int StudentTypeId { get; set; }
		public IList<CourseDto> Courses { get; set; } = new List<CourseDto>();
	}
}
