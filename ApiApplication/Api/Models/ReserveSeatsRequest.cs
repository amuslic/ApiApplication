using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Policy;

namespace ApiApplication.Api.Models
{
    public record ReserveSeatsRequest
    {
        [Required]
        public int ShowtimeId { get; init; }

        [MinLength(1, ErrorMessage = "At least one seat must be specified.")]
        public List<SeatReservationRequest> Seats { get; init; }

        [Required]
        public int AuditoriumId { get; init; }
    }

}
