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

        public ReserveSeatsCommandHandler(
            ITicketsRepository ticketsRepository,
            IAuditoriumsRepository auditoriumsRepository)
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
                throw new NotFoundException(StatusCodes.Status404NotFound, $"Auditorium with id {request.AuditoriumId} doesnt exist");
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

            var isAnySeatUnavailable = request.SeatNumbers.Any(requestedSeat =>
                recentlyReservedSeats.Any(reservedSeat => reservedSeat.Row == requestedSeat.Row && reservedSeat.SeatNumber == requestedSeat.SeatNumber));

            if (isAnySeatUnavailable)
            {
                throw new BadRequestException(StatusCodes.Status400BadRequest, "One or more seats are not available or have been reserved/sold within the last 10 minutes.");
            }

            var selectedSeats = auditorium.Seats.Where(auditoriumSeat =>
                request.SeatNumbers.Any(requestedSeat => requestedSeat.Row == auditoriumSeat.Row && requestedSeat.SeatNumber == auditoriumSeat.SeatNumber)).ToList();

            var reservation = await _ticketsRepository.CreateAsync(showtime, selectedSeats, cancellationToken);

            return reservation.Id;
        }

        private bool AreSeatsContiguous(List<SeatReservation> seatReservations)
        {
            var groupedByRow = seatReservations.GroupBy(sr => sr.Row).ToList();

            foreach (var group in groupedByRow)
            {
                var seatNumbersInRow = group.Select(sr => sr.SeatNumber).OrderBy(n => n).ToList();

                for (int i = 0; i < seatNumbersInRow.Count - 1; i++)
                {
                    if (seatNumbersInRow[i] + 1 != seatNumbersInRow[i + 1])
                        return false;
                }
            }

            return true;
        }
    }
}
