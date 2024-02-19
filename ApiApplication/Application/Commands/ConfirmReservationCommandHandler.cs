using ApiApplication.Database.Repositories.Abstractions;
using MediatR;
using System.Threading.Tasks;
using System.Threading;
using ApiApplication.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using ApiApplication.Application.Abstractions;
using Microsoft.Extensions.Options;
using ApiApplication.Application.Configuration;

namespace ApiApplication.Application.Commands
{
    public class ConfirmReservationCommandHandler : IRequestHandler<ConfirmReservationCommand, Unit>
    {
        private readonly ITicketsRepository _ticketsRepository;
        private readonly ISystemTime _systemTime;
        private readonly int _reservationExpirationTimeInMinutes;

        public ConfirmReservationCommandHandler(
            ITicketsRepository ticketsRepository,
            ISystemTime systemTime,
           IOptionsMonitor<ReservationConfiguration> reservationSettings)
        {
            _ticketsRepository = ticketsRepository;
            _systemTime = systemTime;
            _reservationExpirationTimeInMinutes = reservationSettings.CurrentValue.ReservationExpirationTimeInMinutes;
        }

        public async Task<Unit> Handle(ConfirmReservationCommand request, CancellationToken cancellationToken)
        {
            var ticket = await _ticketsRepository.GetAsync(request.ReservationId, cancellationToken);

            if (ticket is null)
            {
                throw new NotFoundException(StatusCodes.Status404NotFound, $"Reservation with id {request.ReservationId} not found");
            }

            if (ticket.Paid)
            {
                throw new BadRequestException(StatusCodes.Status400BadRequest, $"Reservation with id {request.ReservationId} is already paid");
            }

            if ((_systemTime.UtcNow - ticket.CreatedTime).TotalMinutes > _reservationExpirationTimeInMinutes)
            {
                throw new BadRequestException(StatusCodes.Status400BadRequest, $"Reservation with id {request.ReservationId}has expired and cannot be confirmed");
            }

            ticket.Paid = true;
            await _ticketsRepository.ConfirmPaymentAsync(ticket, cancellationToken);

            return Unit.Value;
        }
    }
}
