using MediatR;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using ApiApplication.Database.Repositories.Abstractions;
using System;
using System.Linq;
using ApiApplication.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using ApiApplication.Application.Models;

namespace ApiApplication.Application.Commands
{
    public class ReserveSeatsCommandHandler : IRequestHandler<ReserveSeatsCommand, Guid>
    {
        private readonly ITicketsRepository _ticketsRepository;
        private readonly IAuditoriumsRepository _auditoriumsRepository;
        private readonly ISystemClock _systemClock; // Interface to abstract system time

        public ReserveSeatsCommandHandler(
            ITicketsRepository ticketsRepository,
            IAuditoriumsRepository auditoriumsRepository,
            ISystemClock systemClock) // Injecting a system clock interface for time abstraction
        {
            _ticketsRepository = ticketsRepository;
            _auditoriumsRepository = auditoriumsRepository;
            _systemClock = systemClock;
        }

        public async Task<Guid> Handle(ReserveSeatsCommand request, CancellationToken cancellationToken)
        {
            if (!AreSeatsContiguous(request.SeatNumbers))
            {
                throw new BadRequestException(StatusCodes.Status400BadRequest, "Seats must be contiguous.");
            }

            var auditorium = await _auditoriumsRepository.GetAsync(request.AuditoriumId, cancellationToken);
            if (auditorium is null)
            {
                throw new NotFoundException(StatusCodes.Status404NotFound, $"Auditorium with ID {request.AuditoriumId} does not exist.");
            }

            var showtime = auditorium.Showtimes?.FirstOrDefault(st => st.Id == request.ShowtimeId);
            if (showtime is null)
            {
                throw new NotFoundException(StatusCodes.Status404NotFound, $"Showtime with ID {request.ShowtimeId} does not exist.");
            }

            var ticketsForShowtime = await _ticketsRepository.GetEnrichedAsync(request.ShowtimeId, cancellationToken);

            var recentlyReservedSeats = ticketsForShowtime
                 .Where(t => (DateTime.Now - t.CreatedTime).TotalMinutes < 10)
                .SelectMany(t => t.Seats)
                .Select(s => (s.Row, s.SeatNumber)) // Project to tuple if necessary
                .ToHashSet();

            var isAnySeatUnavailable = request.SeatNumbers.Any(seat =>
                recentlyReservedSeats.Contains((seat.Row, seat.SeatNumber)));
            if (isAnySeatUnavailable)
            {
                throw new BadRequestException(StatusCodes.Status400BadRequest, "One or more seats are currently unavailable.");
            }

            var selectedSeats = auditorium.Seats.Where(auditoriumSeat =>
                request.SeatNumbers.Any(requestedSeat => requestedSeat.Row == auditoriumSeat.Row && requestedSeat.SeatNumber == auditoriumSeat.SeatNumber)).ToList();

            var reservation = await _ticketsRepository.CreateAsync(showtime, selectedSeats, cancellationToken);

            return reservation.Id;
        }

        private bool AreSeatsContiguous(IEnumerable<SeatReservation> seatReservations)
        {
            var groupedByRow = seatReservations.GroupBy(sr => sr.Row);
            foreach (var group in groupedByRow)
            {
                var seatsInRow = group.Select(sr => sr.SeatNumber).OrderBy(n => n).ToList();
                if (seatsInRow.Zip(seatsInRow.Skip(1), (a, b) => b - a).Any(diff => diff != 1))
                {
                    return false;
                }
            }
            return true;
        }
    }

    public interface ISystemClock
    {
        DateTime UtcNow { get; }
    }
}
