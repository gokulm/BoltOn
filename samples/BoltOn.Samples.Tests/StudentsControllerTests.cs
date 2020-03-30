using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
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
            var response = await httpClient.PostAsJsonAsync($"{_sampleWebApiBaseUrl}students", createStudentRequest);

            // assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.True(response.StatusCode == HttpStatusCode.OK);
            var student = await response.Content.ReadAsJsonAsync<Student>();
            Assert.NotNull(student);
            Assert.Same("John", student.FirstName);
        }
    }

    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> PostAsJsonAsync<T>(
            this HttpClient httpClient, string url, T data)
        {
            var dataAsString = JsonConvert.SerializeObject(data, Formatting.Indented);
            var content = new StringContent(dataAsString, Encoding.UTF8, "application/json");
            return await httpClient.PostAsync(url, content);
        }

        public static async Task<T> ReadAsJsonAsync<T>(this HttpContent content)
        {
            var dataAsString = await content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(dataAsString);
        }
    }
}
