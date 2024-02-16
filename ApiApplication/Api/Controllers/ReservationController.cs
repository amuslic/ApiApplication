using ApiApplication.Api.Models;
using ApiApplication.Application.Commands;
using ApiApplication.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace ApiApplication.Api.Controllers
{
    [Route("api/[controller]")]
    [Tags("Reservations")]
    [ApiController]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public class ReservationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReservationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("reserve-seats")]
        [ProducesResponseType(typeof(ReserveSeatsResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> ReserveSeats([FromBody] ReserveSeatsRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var seatReservations = request.Seats.Select(s => new SeatReservation()
            {
                SeatNumber = s.SeatNumber,
                Row = s.RowNumber
            }).ToList();

            var command = new ReserveSeatsCommand(request.ShowtimeId, seatReservations, request.AuditoriumId);
            var reservationId = await _mediator.Send(command);

            var responseObject = new ReserveSeatsResponse()
            {
                ReservationId = reservationId
            };

            return Ok(responseObject);
        }

        [HttpPost("confirm-reservation")]
        [ProducesResponseType(typeof(ReserveSeatsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConfirmReservation([FromBody] ConfirmReservationRequest request)
        {
            await _mediator.Send(new ConfirmReservationCommand(request.ReservationId));

            return NoContent();
        }
    }
}
