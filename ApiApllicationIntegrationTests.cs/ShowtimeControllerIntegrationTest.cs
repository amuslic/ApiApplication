using ApiApplication.Api.Models;
using ApiApplication.Application.Abstractions;
using ApiApplication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using ProtoDefinitions;
using System.Text;
using Moq;
using System.Net;

namespace ApiApplicationIntegrationTests.cs
{
    public class ShowtimeControllerIntegrationTest : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;
        private readonly Mock<IExternalMovieApiProxy> _movieApiProxyMock = new Mock<IExternalMovieApiProxy>();

        public ShowtimeControllerIntegrationTest(WebApplicationFactory<Startup> factory)
        {
            _client = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddScoped(_ => _movieApiProxyMock.Object);
                });
            }).CreateClient();
        }

        [Fact]
        public async Task Post_CreateShowtime_ReturnsOkWithShowtimeId()
        {
            // Arrange
            var fakeMovieResponse = new showResponse
            {
                Title = "Fake Movie",
                ImDbRating = "8.5",
                Crew = "Fake Director, Fake Actor"
            };
            _movieApiProxyMock.Setup(m => m.GetByIdAsync(It.IsAny<string>()))
                              .ReturnsAsync(fakeMovieResponse);

            var request = new CreateShowtimeRequest
            {
                MovieId = "1",
                SessionDate = DateTime.Now.AddDays(1),
                AuditoriumId = 1
            };

            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/showtime", content);

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task Post_CreateShowtime_ReturnsInternalServerError_WhenExternalServiceFails()
        {
            // Arrange
            _movieApiProxyMock.Setup(m => m.GetByIdAsync(It.IsAny<string>()))
                              .ThrowsAsync(new Exception("External API failure")); 

            var request = new CreateShowtimeRequest
            {
                MovieId = "1",
                SessionDate = DateTime.Now.AddDays(1),
                AuditoriumId = 1
            };

            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/showtime", content);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }


        [Fact]
        public async Task Post_CreateShowtime_ReturnsBadRequest_ForInvalidInput()
        {
            // Arrange
            var request = new CreateShowtimeRequest
            {
                MovieId = "", 
                SessionDate = DateTime.Now.AddDays(-1), 
                AuditoriumId = 0 
            };

            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/showtime", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_CreateShowtime_ReturnsNotFound_WhenMovieNotFound()
        {
            // Arrange
            _movieApiProxyMock.Setup(m => m.GetByIdAsync(It.IsAny<string>()))
                              .ReturnsAsync((showResponse)null); 

            var request = new CreateShowtimeRequest
            {
                MovieId = "999",
                SessionDate = DateTime.Now.AddDays(1),
                AuditoriumId = 1
            };

            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/showtime", content);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
