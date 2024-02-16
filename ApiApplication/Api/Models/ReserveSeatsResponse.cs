using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace ApiApplication.Api.Models
{
    [DataContract(Name = "ReserveSeatsResponse")]
    public class ReserveSeatsResponse
    {
        [JsonProperty]
        public Guid ReservationId { get; init; }
    }
}
