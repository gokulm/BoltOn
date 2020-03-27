using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Xunit;
using BoltOn.Samples.Application.Entities;
using BoltOn.Samples.Application.Handlers;

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
            var response = await httpClient.PostAsync($"{_sampleWebApiBaseUrl}students",
                new StringContent(JsonConvert.SerializeObject(createStudentRequest), System.Text.Encoding.UTF8, 
                    "application/json"));

            // assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.True(response.StatusCode == HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync(); 
            Assert.NotNull(content);
            var student = JsonConvert.DeserializeObject<Student>(content);
            Assert.NotNull(student);
            Assert.Same("John", student.FirstName);
        }
    }
}
