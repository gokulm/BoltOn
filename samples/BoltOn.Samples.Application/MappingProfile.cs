using AutoMapper;
using BoltOn.Samples.Application.DTOs;
using BoltOn.Samples.Application.Entities;

namespace BoltOn.Samples.Application
{
	public class MappingProfile : Profile
	{
		public MappingProfile()
		{
			CreateMap<Student, StudentDto>();
		}
	}
}
