using MediatR;
using System;

namespace ApiApplication.Application.Commands
{
    public class ConfirmReservationCommand : IRequest<(bool IsSuccess, string Message)>
    {
        public Guid ReservationId { get; }

        public ConfirmReservationCommand(Guid reservationId)
        {
            ReservationId = reservationId;
        }
    }
}
