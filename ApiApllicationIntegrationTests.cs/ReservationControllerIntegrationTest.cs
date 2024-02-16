using ApiApplication;
using ApiApplication.Api.Models;
using ApiApplication.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using static ApiApplicationIntegrationTests.cs.Data.SampleDataForReservationTests;

namespace ApiApllicationIntegrationTests.cs
{
    [Collection(nameof(ReservationControllerIntegrationTest))]
    [Trait("Category", "Integration")]
    public class ReservationControllerIntegrationTest : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Startup> _factory;

        public ReservationControllerIntegrationTest(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            SeedDatabase();
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

        [Fact]
        public async Task Post_ReserveSeats_ReturnsOkForValidRequest()
        {
            var request = new
            {
                ShowtimeId = 1,
                SeatNumbers = new List<short> { 3, 4 },
                AuditoriumId = 1
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/Reservation/reserve-seats", stringContent);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Post_ReserveSeats_ReturnsBadRequestForInvalidAuditoriumId()
        {
            var request = new
            {
                ShowtimeId = 1,
                SeatNumbers = new List<short> { 1, 2 },
                AuditoriumId = 999
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/Reservation/reserve-seats", stringContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_ReserveSeats_ReturnsBadRequestForInvalidShowtimeId()
        {
            var request = new
            {
                ShowtimeId = 999,
                SeatNumbers = new List<short> { 3, 4 },
                AuditoriumId = 1
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/Reservation/reserve-seats", stringContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_ReserveSeats_ReturnsBadRequestForNonContiguousSeats()
        {
            var request = new
            {
                ShowtimeId = 1,
                SeatNumbers = new List<short> { 1, 3 },
                AuditoriumId = 1
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/Reservation/reserve-seats", stringContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_ReserveSeats_ReturnsBadRequestForAlreadyReservedSeats()
        {
            var request = new
            {
                ShowtimeId = 1,
                SeatNumbers = new List<short> { 6, 7 },
                AuditoriumId = 1
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            await _client.PostAsync("/api/Reservation/reserve-seats", stringContent);

            var response = await _client.PostAsync("/api/Reservation/reserve-seats", stringContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_ConfirmReservation_ReturnsOkForValidRequest()
        {
            // Arrange
            var request = new
            {
                ShowtimeId = 1,
                SeatNumbers = new List<short> { 3, 4 },
                AuditoriumId = 1
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/Reservation/reserve-seats", stringContent);
            var subscriptionResponse = await response.Content.ReadAsAsync<ReserveSeatsResponse>();

            var confirmReservationRequest = new
            {
                ReservationId = subscriptionResponse.ReservationId
            };

            var confirmReservationRequeststringContent = new StringContent(JsonConvert.SerializeObject(confirmReservationRequest), Encoding.UTF8, "application/json");

            // Act
            var confirmReservationResponse = await _client.PostAsync("/api/Reservation/confirm-reservation", confirmReservationRequeststringContent);

            // Assert
           
            Assert.Equal(HttpStatusCode.OK, confirmReservationResponse.StatusCode);
        }

        [Fact]
        public async Task Post_ConfirmReservation_ReturnsBadRequestForNonExistentReservation()
        {
            var request = new
            {
                ReservationId = 999
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/Reservation/confirm-reservation", stringContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_ConfirmReservation_ReturnsBadRequestForAlreadyConfirmedReservation()
        {
            // Arrange

            var request = new
            {
                ShowtimeId = 1,
                SeatNumbers = new List<short> { 3, 4 },
                AuditoriumId = 1
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/Reservation/reserve-seats", stringContent);

            var confirmReservationRequest = new
            {
                ReservationId = response.Content.ToString(),
            };

            var confirmReservationRequeststringContent = new StringContent(JsonConvert.SerializeObject(confirmReservationRequest), Encoding.UTF8, "application/json");
            await _client.PostAsync("/api/Reservation/confirm-reservation", confirmReservationRequeststringContent);

            // Act
            var confirmReservationSecondAttemptResponse = await _client.PostAsync("/api/Reservation/confirm-reservation", stringContent); // Second confirmation attempt

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, confirmReservationSecondAttemptResponse.StatusCode);
        }

        [Fact]
        public async Task Post_ConfirmReservation_ReturnsBadRequestForExpiredReservation()
        {
            var request = new
            {
                ReservationId = 3 // Assuming this ID exists but the reservation has expired
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/Reservation/confirm-reservation", stringContent);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}