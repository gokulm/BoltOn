using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using BoltOn.Cache;
using BoltOn.Data;
using BoltOn.Requestor.Pipeline;
using BoltOn.Samples.Application.DTOs;
using BoltOn.Samples.Application.Entities;

namespace BoltOn.Samples.Application.Handlers
{
	public class GetAllStudentsRequest : IRequest<IEnumerable<StudentDto>>, ICacheResponse
	{
		public string CacheKey => "Students";

		public TimeSpan? SlidingExpiration => TimeSpan.FromSeconds(45);
	}

	public class GetAllStudentsHandler : IHandler<GetAllStudentsRequest, IEnumerable<StudentDto>>
    {
        private readonly IRepository<Student> _studentRepository;
		private readonly IQueryRepository<StudentType> _studentTypeRepository;
		private readonly IMapper _mapper;

		public GetAllStudentsHandler(IRepository<Student> studentRepository,
			IQueryRepository<StudentType> studentTypeRepository,
			IMapper mapper)
        {
            _studentRepository = studentRepository;
			_studentTypeRepository = studentTypeRepository;
			_mapper = mapper;
		}
        
		public async Task<IEnumerable<StudentDto>> HandleAsync(GetAllStudentsRequest request, 
			CancellationToken cancellationToken = default)
		{
			var studentTypes = await _studentTypeRepository.GetAllAsync(cancellationToken);
			var temp = (await _studentRepository.GetAllAsync(cancellationToken: cancellationToken)).ToList();
			var studentDtos = _mapper.Map<IEnumerable<StudentDto>>(temp);
			foreach(var studentDto in studentDtos)
			{
				studentDto.StudentType = studentTypes.First(f => f.StudentTypeId == studentDto.StudentTypeId).Description;
			}
			return studentDtos;
		}
	}
}
