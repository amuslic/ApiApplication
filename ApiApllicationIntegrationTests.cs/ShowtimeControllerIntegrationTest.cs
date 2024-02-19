using ApiApplication.Api.Models;
using ApiApplication.Application.Abstractions;
using ApiApplication;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using ProtoDefinitions;
using System.Net;
using System.Text;
using Moq;
using ApiApplicationIntegrationTests.Utils;
using ApiApplication.Database;
using static ApiApplicationIntegrationTests.Data.SampleDataForReservationTests;

namespace ApiApplicationIntegrationTests
{
    [Collection(nameof(ShowtimeControllerIntegrationTest))]
    [Trait("Category", "Integration")]
    public class ShowtimeControllerIntegrationTest : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Startup> _factory;
        private readonly Mock<IExternalMovieApiProxy> _movieApiProxyMock = new Mock<IExternalMovieApiProxy>();

        public ShowtimeControllerIntegrationTest(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
            _client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddScoped(_ => _movieApiProxyMock.Object);
                });
            }).CreateClient();
        }

        private void SeedDatabase(int movieId, int auditoriumId, int showtimeId)
        {
            using var scope = _factory.Services.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<CinemaContext>();

            Initialize(db, movieId, auditoriumId, showtimeId);
        }

        private void SetupMockGetByIdAsync(showResponse response = null, bool shouldThrow = false)
        {
            if (shouldThrow)
            {
                _movieApiProxyMock.Setup(m => m.GetByIdAsync(It.IsAny<string>()))
                    .ThrowsAsync(new Exception("External API failure"));
            }
            else
            {
                _movieApiProxyMock.Setup(m => m.GetByIdAsync(It.IsAny<string>()))
                    .ReturnsAsync(response);
            }
        }

        private StringContent CreateRequestContent(object request)
        {
            return new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
        }

        [Fact]
        public async Task Post_CreateShowtime_ReturnsOkWithShowtimeId()
        {
            // Arrange
            Random _random = new();
            var movieId = _random.Next(0, 10000);
            var auditoriumId = _random.Next(0, 10000);
            var showtimeId = _random.Next(0, 10000);

            SeedDatabase(movieId, auditoriumId, showtimeId);

            var fakeMovieResponse = new showResponse {Id = movieId.ToString(), Title = "Fake Movie", ImDbRating = "8.5", Crew = "Fake Director, Fake Actor" };
            SetupMockGetByIdAsync(fakeMovieResponse);

            var request = new CreateShowtimeRequest { MovieId = movieId.ToString(), SessionDate = DateTime.Now.AddDays(1), AuditoriumId = auditoriumId  };
            
            // Act
            var response = await _client.PostAsync("/api/showtime/create-showtime", CreateRequestContent(request));

            // Assert
            response.EnsureSuccessStatusCode();
        }

        [Theory]
        [InlineData("55", true, null, HttpStatusCode.InternalServerError)] // External service failure
        [InlineData("999", false, null, HttpStatusCode.NotFound)] // Movie not found
        public async Task Post_CreateShowtime_VariousScenarios(
            string movieId,
            bool shouldThrow,
            showResponse movieResponse,
            HttpStatusCode expectedStatusCode)
        {
            // Arrange
            SetupMockGetByIdAsync(movieResponse, shouldThrow);

            var request = new CreateShowtimeRequest { MovieId = movieId, SessionDate = DateTime.Now.AddDays(1), AuditoriumId = 1 };
            
            // Act
            var response = await _client.PostAsync("/api/showtime/create-showtime", CreateRequestContent(request));
           
            // Assert
            Assert.Equal(expectedStatusCode, response.StatusCode);
        }
    }
}
