﻿using ApiApplication.Database.Entities;
using MediatR;
using System.Threading.Tasks;
using System.Threading;
using ApiApplication.Database.Repositories.Abstractions;
using ApiApplication.Application.Abstractions;
using ApiApplication.Application.Exceptions;
using Microsoft.AspNetCore.Http;

namespace ApiApplication.Application.Commands
{
    public class CreateShowtimeHandler : IRequestHandler<CreateShowtimeCommand, int>
    {
        private readonly IShowtimesRepository _showtimeRepository;
        private readonly IExternalMovieApiProxy _externalMovieApiProxy;

        public CreateShowtimeHandler(IShowtimesRepository showtimeRepository, IExternalMovieApiProxy externalMovieApiProxy)
        {
            _showtimeRepository = showtimeRepository;
            _externalMovieApiProxy = externalMovieApiProxy;
        }

        public async Task<int> Handle(CreateShowtimeCommand request, CancellationToken cancellationToken)
        {
            var movie = await _externalMovieApiProxy.GetByIdAsync(request.MovieId.ToString());

            if (movie is null)
            {
                throw new NotFoundException(StatusCodes.Status404NotFound, $"Movie with id {request.MovieId} doesnt exist");
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