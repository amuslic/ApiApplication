using ApiApplication;
using ApiApplication.Api.Models;
using ApiApplication.Database;
using ApiApplicationIntegrationTests.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using static ApiApplicationIntegrationTests.Data.SampleDataForReservationTests;

namespace ApiApplicationIntegrationTests
{
    [Collection(nameof(ReservationControllerIntegrationTest))]
    [Trait("Category", "Integration")]
    public class ReservationControllerIntegrationTest : IClassFixture<CustomWebApplicationFactory<Startup>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Startup> _factory;

        public ReservationControllerIntegrationTest(CustomWebApplicationFactory<Startup> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
          
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

        [Theory]
        [InlineData(1, new short[] { 1, 3, 1, 4 }, 1, HttpStatusCode.OK)] // Valid request, contiguous seats in the same row
        [InlineData(1, new short[] { 1, 3, 1, 4 }, 999, HttpStatusCode.NotFound)] // Invalid AuditoriumId, seats in different rows
        [InlineData(999, new short[] { 1, 3, 1, 4 }, 1, HttpStatusCode.NotFound)] // Invalid ShowtimeId, contiguous seats in the same row
        [InlineData(1, new short[] { 1, 1, 1, 3 }, 1, HttpStatusCode.BadRequest)] // Non-Contiguous Seats in the same row
        public async Task Post_ReserveSeats_VariousScenarios(
            int showtimeId,
            short[] seatRowsAndNumbers,
            int auditoriumId, HttpStatusCode expectedStatusCode)
        {
            // Arrange
            SeedDatabase();

            var seats = new List<SeatReservationRequest>();
            for (int i = 0; i < seatRowsAndNumbers.Length; i += 2)
            {
                seats.Add(new SeatReservationRequest { RowNumber = seatRowsAndNumbers[i], SeatNumber = seatRowsAndNumbers[i + 1] });
            }

            var request = new ReserveSeatsRequest
            {
                ShowtimeId = showtimeId,
                Seats = seats,
                AuditoriumId = auditoriumId
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/Reservation/reserve-seats", stringContent);

            // Assert
            Assert.Equal(expectedStatusCode, response.StatusCode);
        }


        [Fact]
        public async Task Post_ReserveSeats_ReturnsBadRequestForAlreadyReservedSeats()
        {
            // Arrange
            SeedDatabase();
            var request = new ReserveSeatsRequest()
            {
                ShowtimeId = 1,
                Seats = [
                  new() { RowNumber = 1, SeatNumber = 2 },
                    new() { RowNumber = 1, SeatNumber = 3 },
                    new() { RowNumber = 2, SeatNumber = 3 },
                    new() { RowNumber = 2, SeatNumber = 4 }],
                AuditoriumId = 1
            };
            var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            await _client.PostAsync("/api/Reservation/reserve-seats", stringContent);

            // Act
            var response = await _client.PostAsync("/api/Reservation/reserve-seats", stringContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_ConfirmReservation_ReturnsOkForValidRequest()
        {
            // Arrange
            SeedDatabase();
            var request = new ReserveSeatsRequest()
            {
                ShowtimeId = 1,
                Seats = [
                    new() { RowNumber = 1, SeatNumber = 2},
                    new() { RowNumber = 1, SeatNumber = 3},
                    new() { RowNumber = 2, SeatNumber = 3 },
                    new() { RowNumber = 2, SeatNumber = 4 }],
                AuditoriumId = 1
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/Reservation/reserve-seats", stringContent);
            var serializedResponse = await response.Content.ReadAsAsync<ReserveSeatsResponse>();

            var confirmReservationRequest = new
            {
                serializedResponse.ReservationId
            };

            var confirmReservationRequeststringContent = new StringContent(JsonConvert.SerializeObject(confirmReservationRequest), Encoding.UTF8, "application/json");

            // Act
            var confirmReservationResponse = await _client.PostAsync("/api/Reservation/confirm-reservation", confirmReservationRequeststringContent);

            // Assert
          
            Assert.Equal(HttpStatusCode.NoContent, confirmReservationResponse.StatusCode);
        }

        [Fact]
        public async Task Post_ConfirmReservation_ReturnsBadRequestForNonExistentReservation()
        {
            // Arrange
            SeedDatabase();
            var request = new
            {
                ReservationId = 999
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
           
            // Act
            var response = await _client.PostAsync("/api/Reservation/confirm-reservation", stringContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Post_ConfirmReservation_ReturnsBadRequestForAlreadyConfirmedReservation()
        {
            // Arrange
            SeedDatabase();
            var request = new ReserveSeatsRequest()
            {
                ShowtimeId = 1,
                Seats = [
                new() { RowNumber = 1, SeatNumber = 2 },
                    new() { RowNumber = 1, SeatNumber = 3 },
                    new() { RowNumber = 2, SeatNumber = 3 },
                    new() { RowNumber = 2, SeatNumber = 4 }],
                AuditoriumId = 1
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/Reservation/reserve-seats", stringContent);
            var serializedResponse = await response.Content.ReadAsAsync<ReserveSeatsResponse>();

            var confirmReservationRequest = new
            {
                serializedResponse.ReservationId
            };

            var confirmReservationRequeststringContent = new StringContent(JsonConvert.SerializeObject(confirmReservationRequest), Encoding.UTF8, "application/json");
            await _client.PostAsync("/api/Reservation/confirm-reservation", confirmReservationRequeststringContent);

            // Act
            var confirmReservationSecondAttemptResponse = await _client.PostAsync("/api/Reservation/confirm-reservation", confirmReservationRequeststringContent); 

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, confirmReservationSecondAttemptResponse.StatusCode);
        }
    }
}