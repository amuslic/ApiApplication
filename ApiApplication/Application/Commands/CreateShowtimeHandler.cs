using ApiApplication.Database.Entities;
using MediatR;
using System.Threading.Tasks;
using System.Threading;
using ApiApplication.Database.Repositories.Abstractions;
using ApiApplication.Application.Abstractions;
using ApiApplication.Application.Exceptions;
using Microsoft.AspNetCore.Http;
using ApiApplication.Database.Repositories;

namespace ApiApplication.Application.Commands
{
    public class CreateShowtimeHandler : IRequestHandler<CreateShowtimeCommand, int>
    {
        private readonly IShowtimesRepository _showtimeRepository;
        private readonly IExternalMovieApiProxy _externalMovieApiProxy;
        private readonly IAuditoriumsRepository _auditoriumsRepository;

        public CreateShowtimeHandler(
            IShowtimesRepository showtimeRepository,
            IExternalMovieApiProxy externalMovieApiProxy,
            IAuditoriumsRepository auditoriumsRepository)
        {
            _showtimeRepository = showtimeRepository;
            _externalMovieApiProxy = externalMovieApiProxy;
            _auditoriumsRepository = auditoriumsRepository;
        }

        public async Task<int> Handle(CreateShowtimeCommand request, CancellationToken cancellationToken)
        {
            var movie = await _externalMovieApiProxy.GetByIdAsync(request.MovieId.ToString());

            var auditorium = await _auditoriumsRepository.GetAsync(request.AuditoriumId, cancellationToken);
            if (auditorium is null)
            {
                throw new NotFoundException(StatusCodes.Status404NotFound, $"Auditorium with id {request.AuditoriumId} doesnt exist");
            }

            var domainMovie = new MovieEntity()
            {
                Title = movie.Title,
                ImdbId = movie.ImDbRating,
                Stars = movie.Crew
            };

            var showtime = new ShowtimeEntity
            {
                Movie = domainMovie,
                SessionDate = request.SessionDate,
                AuditoriumId = request.AuditoriumId
            };

            await _showtimeRepository.CreateShowtime(showtime, cancellationToken);

            return showtime.Id;
        }
    }
}
