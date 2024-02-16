using ProtoDefinitions;
using System.Threading.Tasks;

namespace ApiApplication.Application.Abstractions
{
    public interface IExternalMovieApiProxy
    {
        public Task<showListResponse> GetAllAsync();

        public Task<showResponse> GetByIdAsync(string id);
    }
}
