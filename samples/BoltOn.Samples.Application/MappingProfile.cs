using AutoMapper;
using BoltOn.Samples.Application.DTOs;
using BoltOn.Samples.Application.Entities;

namespace BoltOn.Samples.Application
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			CreateMap<Student, StudentDto>()
				.ForMember(d => d.Courses, o => o.Ignore());
			CreateMap<StudentFlattened, StudentDto>();
			CreateMap<Course, CourseDto>();
		}
	}
}
