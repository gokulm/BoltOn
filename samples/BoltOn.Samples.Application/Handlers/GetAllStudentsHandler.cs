using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BoltOn.Cache;
using BoltOn.Data;
using BoltOn.Requestor.Pipeline;
using BoltOn.Samples.Application.DTOs;
using BoltOn.Samples.Application.Entities;

namespace BoltOn.Samples.Application.Handlers
{
	public class GetAllStudentsRequest : IQuery<IEnumerable<StudentDto>>, ICacheResponse
	{
		public string CacheKey => "Students";

		public TimeSpan? SlidingExpiration => TimeSpan.FromHours(2);
	}

	public class GetAllStudentsHandler : IHandler<GetAllStudentsRequest, IEnumerable<StudentDto>>
    {
        private readonly IRepository<StudentFlattened> _studentRepository;
		private readonly IBoltOnCache _boltOnCache;

		public GetAllStudentsHandler(IRepository<StudentFlattened> studentRepository,
			IBoltOnCache boltOnCache)
        {
            _studentRepository = studentRepository;
			_boltOnCache = boltOnCache;
		}
        
		public async Task<IEnumerable<StudentDto>> HandleAsync(GetAllStudentsRequest request, 
			CancellationToken cancellationToken = default)
		{
			var students = await _boltOnCache.GetAsync("Students", cancellationToken,
				valueGetter: async () => await GetStudents(cancellationToken));
			return students;
		}

		private async Task<IEnumerable<StudentDto>> GetStudents(CancellationToken cancellationToken = default)
		{
			var temp = (await _studentRepository.GetAllAsync(cancellationToken: cancellationToken)).ToList();
			var studentDtos = (from s in temp
							   select new StudentDto
							   {
								   Id = s.Id,
								   FirstName = s.FirstName,
								   LastName = s.LastName,
								   StudentType = s.StudentType
							   }).ToList();

			return studentDtos;
		}
	}
}
