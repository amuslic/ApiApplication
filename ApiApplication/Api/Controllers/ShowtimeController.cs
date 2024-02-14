using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;
using ApiApplication.Application.Commands;
using ApiApplication.Api.Models;

namespace ApiApplication.Api.Controllers;

[ApiController]
[Tags("Showtimes")]
[Route("api/[controller]")]

public class ShowtimeController : ControllerBase
{
    private readonly IMediator _mediator;

    public ShowtimeController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateShowtime([FromBody] CreateShowtimeRequest request)
    {
        try
        {
            // Map the request to the command
            var command = new CreateShowtimeCommand
            {
                MovieId = request.MovieId,
                SessionDate = request.SessionDate,
                AuditoriumId = request.AuditoriumId
            };

            var showtimeId = await _mediator.Send(command);
            return Ok(showtimeId); // Return the ID of the created showtime
        }
        catch (Exception ex)
        {
            // Log the exception
            return StatusCode(500, "An error occurred while creating the showtime: " + ex.Message);
        }
    }
}
