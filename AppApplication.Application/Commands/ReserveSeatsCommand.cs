using AppApplication.Application.Models;
using MediatR;
using System;
using System.Collections.Generic;

namespace AppApplication.Application.Commands
{
    public class ReserveSeatsCommand : IRequest<Guid>
    {
        public int ShowtimeId { get; set; }
        public List<SeatReservation> SeatNumbers { get; set; }
        public int AuditoriumId { get; set; }

        public ReserveSeatsCommand(int showtimeId, List<SeatReservation> seatNumbers, int auditoriumId)
        {
            ShowtimeId = showtimeId;
            SeatNumbers = seatNumbers;
            AuditoriumId = auditoriumId;
        }
    }
}
