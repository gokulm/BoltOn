using System;

namespace BoltOn.Samples.Application.Entities
{
	public class Course
    {
        public virtual Guid CourseId { get; private set; }
        public virtual string CourseName { get; private set; }

        public Course(Guid courseId, string courseName)
		{
            CourseId = courseId;
            CourseName = courseName;
		}
    }
}
