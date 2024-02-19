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

        private void SeedDatabase(int movieId, int auditoriumId, int showtimeId)
        {
            using var scope = _factory.Services.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<CinemaContext>();

            Initialize(db, movieId, auditoriumId, showtimeId);
        }

        [Theory]
        [MemberData(nameof(GetDynamicTestData))]
        public async Task Post_ReserveSeats_VariousScenarios(
            int showtimeId,
            short[] seatRowsAndNumbers,
            int auditoriumId,
            HttpStatusCode expectedStatusCode)
        {
            // Arrange
            SeedDatabase(showtimeId, auditoriumId, showtimeId); 

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
        public async Task Post_ReserveSeats_ReturnsNotFoundForNotExistingAuditorium()
        {
            // Arrange
            Random _random = new();
            var movieId = _random.Next(0, 10000);
            var auditoriumId = _random.Next(2, 10000);
            var showtimeId = _random.Next(0, 10000);
            SeedDatabase(movieId, auditoriumId, showtimeId);

            var request = new ReserveSeatsRequest()
            {
                ShowtimeId = showtimeId,
                Seats = [
                  new() { RowNumber = 1, SeatNumber = 2 },
                    new() { RowNumber = 1, SeatNumber = 3 },
                    new() { RowNumber = 2, SeatNumber = 3 },
                    new() { RowNumber = 2, SeatNumber = 4 }],
                AuditoriumId = 1
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/Reservation/reserve-seats", stringContent);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Post_ReserveSeats_ReturnsNotFoundForNotExistingShowtime()
        {
            // Arrange
            Random _random = new();
            var movieId = _random.Next(0, 10000);
            var auditoriumId = _random.Next(0, 10000);
            var showtimeId = _random.Next(2, 10000);
            SeedDatabase(movieId, auditoriumId, showtimeId);

            var request = new ReserveSeatsRequest()
            {
                ShowtimeId = 1,
                Seats = [
                  new() { RowNumber = 1, SeatNumber = 2 },
                    new() { RowNumber = 1, SeatNumber = 3 },
                    new() { RowNumber = 2, SeatNumber = 3 },
                    new() { RowNumber = 2, SeatNumber = 4 }],
                AuditoriumId = auditoriumId
            };

            var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/Reservation/reserve-seats", stringContent);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Post_ReserveSeats_ReturnsBadRequestForAlreadyReservedSeats()
        {
            // Arrange
            Random _random = new();
            var movieId = _random.Next(0, 10000);
            var auditoriumId = _random.Next(0, 10000);
            var showtimeId = _random.Next(0, 10000);
            SeedDatabase(movieId, auditoriumId, showtimeId);

            var request = new ReserveSeatsRequest()
            {
                ShowtimeId = showtimeId,
                Seats = [
                  new() { RowNumber = 1, SeatNumber = 2 },
                    new() { RowNumber = 1, SeatNumber = 3 },
                    new() { RowNumber = 2, SeatNumber = 3 },
                    new() { RowNumber = 2, SeatNumber = 4 }],
                AuditoriumId = auditoriumId
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
            Random _random = new();
            var movieId = _random.Next(0, 10000);
            var auditoriumId = _random.Next(0, 10000);
            var showtimeId = _random.Next(0, 10000);
            SeedDatabase(movieId, auditoriumId, showtimeId);

            var request = new ReserveSeatsRequest()
            {
                ShowtimeId = showtimeId,
                Seats = [
                    new() { RowNumber = 1, SeatNumber = 2 },
                    new() { RowNumber = 1, SeatNumber = 3 },
                    new() { RowNumber = 2, SeatNumber = 3 },
                    new() { RowNumber = 2, SeatNumber = 4 }],
                AuditoriumId = auditoriumId
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
            Random _random = new();
            var movieId = _random.Next(0, 10000);
            var auditoriumId = _random.Next(0, 10000);
            var showtimeId = _random.Next(0, 10000);
            SeedDatabase(movieId, auditoriumId, showtimeId);
            var request = new ReserveSeatsRequest()
            {
                ShowtimeId = showtimeId,
                Seats = [
               new() { RowNumber = 1, SeatNumber = 2 },
                    new() { RowNumber = 1, SeatNumber = 3 },
                    new() { RowNumber = 2, SeatNumber = 3 },
                    new() { RowNumber = 2, SeatNumber = 4 }],
                AuditoriumId = auditoriumId
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

        public static IEnumerable<object[]> GetDynamicTestData()
        {
            var random = new Random();
            yield return new object[] { random.Next(1, 999), new short[] { 1, 3, 1, 4 }, random.Next(1, 999), HttpStatusCode.OK };
            yield return new object[] { random.Next(1, 999), new short[] { 1, 1, 1, 3 }, random.Next(1, 999), HttpStatusCode.BadRequest };
        }
    }
}