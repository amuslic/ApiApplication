namespace ApiApplication.Client.Requests
{
    public record SeatReservationRequest
    {
        public short RowNumber { get; init; }

        public short SeatNumber { get; init; }
    }
}
