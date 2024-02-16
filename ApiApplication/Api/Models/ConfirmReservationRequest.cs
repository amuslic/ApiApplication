using System;
using System.ComponentModel.DataAnnotations;

namespace ApiApplication.Api.Models
{
    public class ConfirmReservationRequest
    {

        [Required]
        public Guid ReservationId { get; set; }
    }
}
