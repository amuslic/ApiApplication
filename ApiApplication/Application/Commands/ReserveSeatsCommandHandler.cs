using MediatR;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using ApiApplication.Database.Repositories.Abstractions;
using System;
using System.Linq;

namespace ApiApplication.Application.Commands
{
    public class ReserveSeatsCommandHandler : IRequestHandler<ReserveSeatsCommand, (bool IsSuccess, string ReservationId, string ErrorMessage)>
    {
        private readonly ITicketsRepository _ticketsRepository;
        private readonly IAuditoriumsRepository _auditoriumsRepository;

        public ReserveSeatsCommandHandler(
            ITicketsRepository ticketsRepository, IAuditoriumsRepository auditoriumsRepository)
        {
            _ticketsRepository = ticketsRepository;
            _auditoriumsRepository = auditoriumsRepository;
        }

        public async Task<(bool IsSuccess, string ReservationId, string ErrorMessage)> Handle(ReserveSeatsCommand request, CancellationToken cancellationToken)
        {
            if (!AreSeatsContiguous(request.SeatNumbers))
            {
                return (false, null, "Seats need to be contiguous.");
            }

            var auditorium = await _auditoriumsRepository.GetAsync(request.AuditoriumId, cancellationToken);
            if (auditorium == null)
            {
                return (false, null, "Invalid auditorium ID.");
            }

            var showtime = auditorium.Showtimes?.FirstOrDefault(st => st.Id == request.ShowtimeId);
            if (showtime == null)
            {
                return (false, null, "Invalid showtime ID.");
            }

            // Fetch all tickets for the requested showtime to check their associated seats
            var ticketsForShowtime = await _ticketsRepository.GetEnrichedAsync(request.ShowtimeId, cancellationToken);

            // Flatten all seats from these tickets and filter by the created time to identify recently reserved/sold seats
            var recentlyReservedSeats = ticketsForShowtime
                .Where(t => (DateTime.Now - t.CreatedTime).TotalMinutes < 10) // Filtering tickets created within the last 10 minutes
                .SelectMany(t => t.Seats) // Selecting all seats associated with these recent tickets
                .ToList();

            // Check if any of the requested seat numbers are among the recently reserved/sold seats
            var isAnySeatUnavailable = request.SeatNumbers.Any(seatNumber =>
                recentlyReservedSeats.Any(s => s.SeatNumber == seatNumber));

            if (isAnySeatUnavailable)
            {
                return (false, null, "One or more seats are not available or have been reserved/sold within the last 10 minutes.");
            }

            // Convert seat numbers to SeatEntity list
            var selectedSeats = auditorium.Seats.Where(s => request.SeatNumbers.Contains(s.SeatNumber)).ToList();

            // Create the reservation
            var reservation = await _ticketsRepository.CreateAsync(showtime, selectedSeats, cancellationToken);

            // Here you might also trigger a background task for handling reservation expiration

            return (true, reservation.Id.ToString(), $"Reserved {selectedSeats.Count} seats for Showtime {showtime.Id} in Auditorium {auditorium.Id}.");
        }

        private bool AreSeatsContiguous(List<short> seatNumbers)
        {
            seatNumbers.Sort();
            for (int i = 0; i < seatNumbers.Count - 1; i++)
            {
                if (seatNumbers[i] + 1 != seatNumbers[i + 1])
                    return false;
            }
            return true;
        }
    }
}
