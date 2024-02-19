using Grpc.Core;
using System.Linq;
using MediaR;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using GrpcService2; // Update this using statement based on your actual Protobuf generated code namespace

public class ReservationService : ReservationService.ReservationServiceBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ReservationService> _logger;

    public ReservationService(IMediator mediator, ILogger<ReservationService> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public override async Task<ReserveSeatsResponse> ReserveSeats(ReserveSeatsRequest request, ServerCallContext context)
    {
        var seatReservations = request.Seats.Select(s => new SeatReservation
        {
            SeatNumber = s.SeatNumber,
            Row = s.Row
        }).ToList();

        var command = new ReserveSeatsCommand(request.ShowtimeId, seatReservations, request.AuditoriumId);
        var reservationId = await _mediator.Send(command);

        return new ReserveSeatsResponse
        {
            ReservationId = reservationId
        };
    }

    public override async Task<Google.Protobuf.WellKnownTypes.Empty> ConfirmReservation(ConfirmReservationRequest request, ServerCallContext context)
    {
        await _mediator.Send(new ConfirmReservationCommand(request.ReservationId));
        return new Google.Protobuf.WellKnownTypes.Empty();
    }
}
