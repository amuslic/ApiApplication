using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace ApiApplication.Api.Models
{
    [DataContract(Name = "CreateShowtimeResponse")]
    public class CreateShowtimeResponse
    {
        [JsonProperty]
        public int ShowtimeId { get; init; }
    }
}
