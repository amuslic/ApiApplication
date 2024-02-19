using Google.Protobuf;
using System.Threading.Tasks;

namespace AppApplication.Application.Abstractions
{
    public interface ICacheService
    {
        Task<T> GetAsync<T>(string key) where T : IMessage<T>, new();

        Task SetAsync<T>(string key, T value) where T : IMessage<T>;
    }
}
