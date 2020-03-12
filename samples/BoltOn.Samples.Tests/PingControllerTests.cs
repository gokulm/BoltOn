using System;
using System.Net.Http;
using Xunit;

namespace BoltOn.Samples.Tests
{
    public class PingControllerTests
    {
        [Fact]
        public void Get_PingRequest_ReturnsPongResponse()
        {
            // arrange
            var httpClient = new HttpClient();

            // act

            // assert
        }
    }
}
