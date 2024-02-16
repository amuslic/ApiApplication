using ApiApplication.Database.Repositories.Abstractions;
using MediatR;
using System.Threading.Tasks;
using System.Threading;
using System;
using ApiApplication.Application.Exceptions;
using Microsoft.AspNetCore.Http;

namespace ApiApplication.Application.Commands
{
    public class ConfirmReservationCommandHandler : IRequestHandler<ConfirmReservationCommand, (bool IsSuccess, string Message)>
    {
        private readonly ITicketsRepository _ticketsRepository;

        public ConfirmReservationCommandHandler(ITicketsRepository ticketsRepository)
        {
            _ticketsRepository = ticketsRepository;
        }

        public async Task<(bool IsSuccess, string Message)> Handle(ConfirmReservationCommand request, CancellationToken cancellationToken)
        {
            var ticket = await _ticketsRepository.GetAsync(request.ReservationId, cancellationToken);

            if (ticket is null)
            {
                throw new NotFoundException(StatusCodes.Status404NotFound, "Reservation with id {request.ReservationId} not found");
            }

            if (ticket.Paid)
            {
                throw new BadRequestException(StatusCodes.Status400BadRequest, "Reservation with id {request.ReservationId} is already paid");
            }

            if ((DateTime.Now - ticket.CreatedTime).TotalMinutes > 10)
            {
                throw new BadRequestException(StatusCodes.Status400BadRequest, "Reservation with id {request.ReservationId}has expired and cannot be confirmed");
            }

            ticket.Paid = true;
            await _ticketsRepository.ConfirmPaymentAsync(ticket, cancellationToken);

            return (true, "Reservation confirmed successfully.");
        }
    }
}
