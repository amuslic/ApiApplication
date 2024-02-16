using System;
using System.ComponentModel.DataAnnotations;

namespace ApiApplication.Api.Models
{
    public class CreateShowtimeRequest
    {

        [Required]
        public string MovieId { get; set; }
        public DateTime SessionDate { get; set; }

        [Required]
        public int AuditoriumId { get; set; }
    }
}
