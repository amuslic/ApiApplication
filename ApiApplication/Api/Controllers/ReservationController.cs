﻿using ApiApplication.Api.Models;
using ApiApplication.Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ApiApplication.Api.Controllers
{
    [Route("api/[controller]")]
    [Tags("Reservations")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReservationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("reserve-seats")]
        [ProducesResponseType(typeof(ReserveSeatsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ReserveSeats([FromBody] ReserveSeatsRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var command = new ReserveSeatsCommand(request.ShowtimeId, request.SeatNumbers, request.AuditoriumId);
            var (IsSuccess, ReservationId, ErrorMessage) = await _mediator.Send(command);

            var responseObject = new ReserveSeatsResponse()
            {
                ReservationId = Guid.Parse(ReservationId)
            };

                return Ok(responseObject);
        }

        [HttpPost("confirm-reservation")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ConfirmReservation([FromBody] ConfirmReservationRequest request)
        {
            var (IsSuccess, Message) = await _mediator.Send(new ConfirmReservationCommand(request.ReservationId));

            return Ok(new { Message });

        }
    }
}