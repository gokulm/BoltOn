using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace BoltOn.Samples.Tests
{
    public class PingControllerTests
    {
        [Fact]
        public async Task Get_PingRequest_ReturnsPongResponse()
        {
            // arrange
            var httpClient = new HttpClient();

            // act
            var response = await httpClient.GetAsync("http://localhost:5000/ping");

            // assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.True(response.StatusCode == HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Equal("pong", content);
        }
    }
}
