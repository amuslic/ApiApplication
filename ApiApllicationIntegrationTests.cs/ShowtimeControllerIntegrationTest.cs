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
using Microsoft.AspNetCore.Builder;
using static ApiApplicationIntegrationTests.Data.SampleDataForReservationTests;

namespace ApiApplicationIntegrationTests
{
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

        private void SeedDatabase()
        {
            using var scope = _factory.Services.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<CinemaContext>();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            Initialize(new ApplicationBuilder(scopedServices));
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
            SeedDatabase();
            var fakeMovieResponse = new showResponse {Id = "4", Title = "Fake Movie", ImDbRating = "8.5", Crew = "Fake Director, Fake Actor" };
            SetupMockGetByIdAsync(fakeMovieResponse);

            var request = new CreateShowtimeRequest { MovieId = "4", SessionDate = DateTime.Now.AddDays(1), AuditoriumId = 1 };
            
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
