using ApiApplication.Database.Entities;
using ApiApplication.Database;

namespace ApiApplicationIntegrationTests.Data
{
    public static class SampleDataForReservationTests
    {
        public static void Initialize(CinemaContext context, int movieId, int auditoriumId, int showtimeId)
        {
            if (!context.Auditoriums.Any(a => a.Id == auditoriumId))
            {
                var auditorium = new AuditoriumEntity
                {
                    Id = auditoriumId,
                    Showtimes = new List<ShowtimeEntity>
                    {
                        new ShowtimeEntity {
                            Id = showtimeId,
                            SessionDate = DateTime.Now.AddDays(1),
                           Movie = new MovieEntity
                            {
                                Id = movieId,
                                Title = "Inception",
                                ImdbId = "tt1375666",
                                ReleaseDate = new DateTime(2010, 1, 14),
                                Stars = "Leonardo DiCaprio, Joseph Gordon-Levitt, Ellen Page, Ken Watanabe"
                            },
                            AuditoriumId = auditoriumId,
                            Tickets = new List<TicketEntity>()
                        }
                    },
                    Seats = GenerateSeats(auditoriumId, 5, 10)
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

