using System;

namespace ApiApplication.Api.Models
{
    public class CreateShowtimeRequest
    {
        public string MovieId { get; set; }
        public DateTime SessionDate { get; set; }
        public int AuditoriumId { get; set; }
    }
}
