using ApiApplication.Database.Repositories.Abstractions;
using MediatR;
using System.Threading.Tasks;
using System.Threading;
using System;

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

            if (ticket == null)
            {
                return (false, "Reservation not found.");
            }

            if (ticket.Paid)
            {
                return (false, "This reservation has already been confirmed.");
            }

            if ((DateTime.Now - ticket.CreatedTime).TotalMinutes > 10)
            {
                return (false, "Reservation has expired and cannot be confirmed.");
            }

            ticket.Paid = true;
            await _ticketsRepository.ConfirmPaymentAsync(ticket, cancellationToken);

            return (true, "Reservation confirmed successfully.");
        }
    }
}
