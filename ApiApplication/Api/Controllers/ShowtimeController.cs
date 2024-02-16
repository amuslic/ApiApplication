using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Threading.Tasks;
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

    [HttpPost("create-showtime")]
    [ProducesResponseType(typeof(CreateShowtimeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateShowtime([FromBody] CreateShowtimeRequest request)
    {
        var command = new CreateShowtimeCommand
        {
            MovieId = request.MovieId,
            SessionDate = request.SessionDate,
            AuditoriumId = request.AuditoriumId
        };

        var showtimeId = await _mediator.Send(command);
        var responseObject = new CreateShowtimeResponse()
        {
            ShowtimeId = showtimeId
        };

        return Ok(responseObject);
    }
}
