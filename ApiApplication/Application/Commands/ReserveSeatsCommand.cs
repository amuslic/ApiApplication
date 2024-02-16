using MediatR;
using System;
using System.Collections.Generic;

namespace ApiApplication.Application.Commands
{
    public class ReserveSeatsCommand : IRequest<Guid>
    {
        public int ShowtimeId { get; set; }
        public List<short> SeatNumbers { get; set; }
        public int AuditoriumId { get; set; }

        public ReserveSeatsCommand(int showtimeId, List<short> seatNumbers, int auditoriumId)
        {
            ShowtimeId = showtimeId;
            SeatNumbers = seatNumbers;
            AuditoriumId = auditoriumId;
        }
    }
}
