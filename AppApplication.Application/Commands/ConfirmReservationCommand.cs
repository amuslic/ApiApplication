using MediatR;
using System;

namespace AppApplication.Application.Commands
{
    public class ConfirmReservationCommand : IRequest<Unit>
    {
        public Guid ReservationId { get; }

        public ConfirmReservationCommand(Guid reservationId)
        {
            ReservationId = reservationId;
        }
    }
}
