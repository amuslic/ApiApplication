using ProtoDefinitions;
using System.Threading.Tasks;

namespace AppApplication.Application.Abstractions
{
    public interface IExternalMovieApiProxy
    {
        public Task<showListResponse> GetAllAsync();

        public Task<showResponse> GetByIdAsync(string id);
    }
}
