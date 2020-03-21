using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace BoltOn.Samples.Tests
{
    public class PingControllerTests
    {
        private readonly bool _isIntegrationTestsEnabled;
        private readonly string _sampleWebApiBaseUrl;

        public PingControllerTests()
        {
            var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json");
            var configuration = builder.Build();
            _isIntegrationTestsEnabled = configuration.GetValue<bool>("IsIntegrationTestsEnabled");
            _sampleWebApiBaseUrl = configuration.GetValue<string>("SamplesWebApiBaseUrl");
        }

        [Fact]
        public async Task Get_PingRequest_ReturnsPongResponse()
        {
            // arrange
            if(!_isIntegrationTestsEnabled)
                return;
            var httpClient = new HttpClient();

            // act
            var response = await httpClient.GetAsync($"{_sampleWebApiBaseUrl}ping");

            // assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.True(response.StatusCode == HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("pong", content);
        }
    }
}
