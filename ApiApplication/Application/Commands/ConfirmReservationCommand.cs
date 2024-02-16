using MediatR;
using System;

namespace ApiApplication.Application.Commands
{
    public class ConfirmReservationCommand : IRequest
    {
        public Guid ReservationId { get; }

        public ConfirmReservationCommand(Guid reservationId)
        {
            ReservationId = reservationId;
        }
    }
}
