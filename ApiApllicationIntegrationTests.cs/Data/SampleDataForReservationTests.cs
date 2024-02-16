using ApiApplication.Database.Entities;
using ApiApplication.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ApiApplicationIntegrationTests.Data
{
    public static class SampleDataForReservationTests
    {
        public static void Initialize(IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var context = serviceScope.ServiceProvider.GetService<CinemaContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            if (!context.Auditoriums.Any())
            {
                var auditorium = new AuditoriumEntity
                {
                    Id = 1,
                    Showtimes = new List<ShowtimeEntity>
                    {
                        new() {
                            Id = 1,
                            SessionDate = DateTime.Now.AddDays(1), 
                            Movie = new MovieEntity
                            {
                                Id = 1,
                                Title = "Inception",
                                ImdbId = "tt1375666",
                                ReleaseDate = new DateTime(2010, 1, 14),
                                Stars = "Leonardo DiCaprio, Joseph Gordon-Levitt, Ellen Page, Ken Watanabe"
                            },
                            AuditoriumId = 1,
                            Tickets = new List<TicketEntity>()                          
                        }
                    },
                    Seats = GenerateSeats(1, 5, 10) 
                };

                context.Auditoriums.Add(auditorium);
                context.SaveChanges();
            }
        }

        private static List<SeatEntity> GenerateSeats(int auditoriumId, short rows, short seatsPerRow)
        {
            var seats = new List<SeatEntity>();
            for (short r = 1; r <= rows; r++)
            {
                for (short s = 1; s <= seatsPerRow; s++)
                {
                    seats.Add(new SeatEntity { AuditoriumId = auditoriumId, Row = r, SeatNumber = s });
                }
            }
            return seats;
        }
    }
}
