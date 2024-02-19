using ApiApplication.Infrastructure.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace ApiApplication.Infrastructure.Repositories.Abstractions
{
    public interface IAuditoriumsRepository
    {
        Task<AuditoriumEntity> GetAsync(int auditoriumId, CancellationToken cancel);
    }
}