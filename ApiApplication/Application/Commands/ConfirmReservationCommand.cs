using MediatR;
using System;

namespace ApiApplication.Application.Commands
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
