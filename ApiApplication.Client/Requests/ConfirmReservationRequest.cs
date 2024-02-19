using System;
using System.ComponentModel.DataAnnotations;

namespace ApiApplication.Client.Requests
{
    public class ConfirmReservationRequest
    {
        [Required]
        public Guid ReservationId { get; set; }
    }
}
