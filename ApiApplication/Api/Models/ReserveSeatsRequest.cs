using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ApiApplication.Api.Models
{
    public class ReserveSeatsRequest
    {
        public int ShowtimeId { get; set; }

        [MinLength(1, ErrorMessage = "At least one seat number must be specified.")]
        public List<short> SeatNumbers { get; set; }

        public int AuditoriumId { get; set; }
    }

}
