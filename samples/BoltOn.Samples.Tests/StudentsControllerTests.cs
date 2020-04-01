using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Xunit;
using BoltOn.Samples.Application.Entities;
using BoltOn.Samples.Application.Handlers;
using System.Collections.Generic;
using System.Linq;
using BoltOn.Samples.Application.DTOs;

namespace BoltOn.Samples.Tests
{
	public class StudentsControllerTests
    {
        private readonly bool _isIntegrationTestsEnabled;
        private readonly string _sampleWebApiBaseUrl;

        public StudentsControllerTests()
        {
            var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json");
            var configuration = builder.Build();
            _isIntegrationTestsEnabled = configuration.GetValue<bool>("IsIntegrationTestsEnabled");
            _sampleWebApiBaseUrl = configuration.GetValue<string>("SamplesWebApiBaseUrl");
        }

        [Fact]
        public async Task Post_AddStudent_AddsStudentAndReturnsAddedStudent()
        {
            // arrange
            if(!_isIntegrationTestsEnabled)
                return;
            var httpClient = new HttpClient();
            var createStudentRequest = new CreateStudentRequest
            {
                FirstName = "John",
                LastName = "Smith",
                StudentTypeId = 1
            };

			// act
			var response = await httpClient.PostAsJsonAsync($"{_sampleWebApiBaseUrl}students", createStudentRequest);

			// assert
			Assert.True(response.IsSuccessStatusCode);
            Assert.True(response.StatusCode == HttpStatusCode.OK);
			var student = await response.Content.ReadAsJsonAsync<StudentDto>();
			Assert.NotNull(student);
			Assert.Equal("John", student.FirstName);
            await Task.Delay(1000);
            await TestGetAllStudents(httpClient);
		}
        
        private async Task TestGetAllStudents(HttpClient httpClient)
        {
            // act
            var response = await httpClient.GetAsync($"{_sampleWebApiBaseUrl}students");

            // assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.True(response.StatusCode == HttpStatusCode.OK);
            var students = await response.Content.ReadAsJsonAsync<IEnumerable<Student>>();
            Assert.NotNull(students);
			Assert.True(students.Count() > 0);
		}
    }
}
