using MediatR;
using Microsoft.AspNetCore.Http;
using AppApplication.Application.Abstractions;
using ApiApplication.Infrastructure.Repositories.Abstractions;
using AppApplication.Application.Exceptions;

namespace AppApplication.Application.Commands
{
    public class ConfirmReservationCommandHandler : IRequestHandler<ConfirmReservationCommand, Unit>
    {
        private readonly ITicketsRepository _ticketsRepository;
        private readonly ISystemTime _systemTime;

        public ConfirmReservationCommandHandler(
            ITicketsRepository ticketsRepository,
            ISystemTime systemTime)
        {
            _ticketsRepository = ticketsRepository;
            _systemTime = systemTime;
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

            if ((_systemTime.Now - ticket.CreatedTime).TotalMinutes > 10)
            {
                throw new BadRequestException(StatusCodes.Status400BadRequest, $"Reservation with id {request.ReservationId}has expired and cannot be confirmed");
            }

            ticket.Paid = true;
            await _ticketsRepository.ConfirmPaymentAsync(ticket, cancellationToken);

            return Unit.Value;
        }
    }
}
