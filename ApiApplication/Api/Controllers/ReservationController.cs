using ApiApplication.Api.Models;
using ApiApplication.Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ApiApplication.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReservationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("reserve-seats")]
        public async Task<IActionResult> ReserveSeats([FromBody] ReserveSeatsApiRequest request)
        {
            // Validate the API request
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Convert API request to command
            var command = new ReserveSeatsCommand(request.ShowtimeId, request.SeatNumbers, request.AuditoriumId);

            // Send command to mediator
            var (IsSuccess, ReservationId, ErrorMessage) = await _mediator.Send(command);

            if (IsSuccess)
            {
                return Ok(new { ReservationId = ReservationId });
            }

            return BadRequest(new { Error = ErrorMessage });
        }

        [HttpPost("confirm-reservation")]
        public async Task<IActionResult> ConfirmReservation([FromBody] ConfirmReservationApiRequest request)
        {
            var (IsSuccess, Message) = await _mediator.Send(new ConfirmReservationCommand(request.ReservationId));

            if (IsSuccess)
            {
                return Ok(new { Message });
            }

            return BadRequest(new { Message });
        }
    }
}
