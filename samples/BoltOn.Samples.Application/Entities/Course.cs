using System;

namespace BoltOn.Samples.Application.Entities
{
    public class Course
    {
        public virtual Guid CourseId { get; set; }
        public virtual string CourseName { get; set; }
    }

    public class StudentCourse 
    {
		public virtual Guid StudentCourseId { get; private set; }
		public virtual Guid StudentId { get; private set; }
        public virtual Guid CourseId { get; private set; }

        protected StudentCourse()
        {
        }

        public StudentCourse(Guid studentId, Guid courseId)
        {
            StudentCourseId = Guid.NewGuid();
            StudentId = studentId;
            CourseId = courseId;
        }
    }
}
