using MediatR;
using System;

namespace ApiApplication.Application.Commands
{
    public class CreateShowtimeCommand : IRequest<int>
    {
        public string MovieId { get; set; }
        public DateTime SessionDate { get; set; }
        public int AuditoriumId { get; set; }
    }
}
