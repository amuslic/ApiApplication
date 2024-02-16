using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ApiApplication.Api.Models
{
    public record ReserveSeatsRequest
    {
        public int ShowtimeId { get; init; }

        [MinLength(1, ErrorMessage = "At least one seat number must be specified.")]
        public List<SeatReservationRequest> Seats { get; init; }

        public int AuditoriumId { get; init; }
    }

}
