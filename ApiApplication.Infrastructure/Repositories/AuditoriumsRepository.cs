using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Threading;
using ApiApplication.Infrastructure;
using ApiApplication.Infrastructure.Entities;
using ApiApplication.Infrastructure.Repositories.Abstractions;

namespace ApiApplication.Infrastructure.Repositories
{
    public class AuditoriumsRepository : IAuditoriumsRepository
    {
        private readonly CinemaContext _context;

        public AuditoriumsRepository(CinemaContext context)
        {
            _context = context;
        }

        public async Task<AuditoriumEntity> GetAsync(int auditoriumId, CancellationToken cancel)
        {
            return await _context.Auditoriums
                .Include(x => x.Seats)
                .Include(x => x.Showtimes)
                .FirstOrDefaultAsync(x => x.Id == auditoriumId, cancel);
        }
    }
}
