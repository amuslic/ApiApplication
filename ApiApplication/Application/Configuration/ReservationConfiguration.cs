namespace ApiApplication.Application.Configuration
{
    public record ReservationConfiguration
    {
        public static readonly string ConfigurationKey = "Reservation";
        public int ReservationExpirationTimeInMinutes { get; init; }
    }
}
