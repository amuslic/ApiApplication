using AppApplication.Application.Commands;
using Grpc.Core;
using MediatR;
using Reservation;
using static Reservation.ReservationService;


namespace ApiApplication.Grpc.Services
{
    public class ReservationService : ReservationServiceBase
    {
        private readonly ILogger<ReservationService> _logger;
        private readonly IMediator _mediator;

        public ReservationService(ILogger<ReservationService> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public override async Task<ReserveSeatsResponse> ReserveSeats(ReserveSeatsRequest request, ServerCallContext context)
        {
            // Convert gRPC request to your application's command request
            var seatReservations = request.Seats.Select(s => new AppApplication.Application.Models.SeatReservation
            {
                SeatNumber = (short)s.SeatNumber,
                Row = (short)s.Row
            }).ToList();

            var command = new AppApplication.Application.Commands.ReserveSeatsCommand(
                request.ShowtimeId,
                seatReservations,
                request.AuditoriumId);

            var reservationId = await _mediator.Send(command);

            return new ReserveSeatsResponse
            {
                ReservationId = reservationId.ToString()
            };
        }

        public override async Task<ConfirmReservationResponse> ConfirmReservation(ConfirmReservationRequest request, ServerCallContext context)
        {
            if (!Guid.TryParse(request.ReservationId, out Guid reservationIdGuid))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid reservation ID format"));
            }

            var command = new ConfirmReservationCommand(reservationIdGuid);

            await _mediator.Send(command);

            return new ConfirmReservationResponse();
        }

    }
}
