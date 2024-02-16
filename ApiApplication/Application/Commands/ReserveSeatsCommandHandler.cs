using MediatR;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using ApiApplication.Database.Repositories.Abstractions;
using System;
using System.Linq;
using ApiApplication.Application.Exceptions;
using Microsoft.AspNetCore.Http;

namespace ApiApplication.Application.Commands
{
    public class ReserveSeatsCommandHandler : IRequestHandler<ReserveSeatsCommand, Guid>
    {
        private readonly ITicketsRepository _ticketsRepository;
        private readonly IAuditoriumsRepository _auditoriumsRepository;

        public ReserveSeatsCommandHandler(
            ITicketsRepository ticketsRepository, IAuditoriumsRepository auditoriumsRepository)
        {
            _ticketsRepository = ticketsRepository;
            _auditoriumsRepository = auditoriumsRepository;
        }

        public async Task<Guid> Handle(ReserveSeatsCommand request, CancellationToken cancellationToken)
        {
            if (!AreSeatsContiguous(request.SeatNumbers))
            {
                throw new BadRequestException(StatusCodes.Status400BadRequest, "Seats need to be contiguous");
            }

            var auditorium = await _auditoriumsRepository.GetAsync(request.AuditoriumId, cancellationToken);
            if (auditorium is null)
            {
                throw new NotFoundException(StatusCodes.Status404NotFound, $"Auditorum with id {request.AuditoriumId} doesnt exist");
            }

            var showtime = auditorium.Showtimes?.FirstOrDefault(st => st.Id == request.ShowtimeId);
            if (showtime is null)
            {
                throw new NotFoundException(StatusCodes.Status404NotFound, $"Showtime with id {request.ShowtimeId} doesnt exist");
            }

            var ticketsForShowtime = await _ticketsRepository.GetEnrichedAsync(request.ShowtimeId, cancellationToken);

            var recentlyReservedSeats = ticketsForShowtime
                .Where(t => (DateTime.Now - t.CreatedTime).TotalMinutes < 10)
                .SelectMany(t => t.Seats)
                .ToList();

            var isAnySeatUnavailable = request.SeatNumbers.Any(seatNumber =>
                recentlyReservedSeats.Any(s => s.SeatNumber == seatNumber));

            if (isAnySeatUnavailable)
            {
                throw new BadRequestException(StatusCodes.Status400BadRequest, "One or more seats are not available or have been reserved/sold within the last 10 minutes.");
            }

            var selectedSeats = auditorium.Seats.Where(s => request.SeatNumbers.Contains(s.SeatNumber)).ToList();

            var reservation = await _ticketsRepository.CreateAsync(showtime, selectedSeats, cancellationToken);

            return reservation.Id;
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
