using ApiApplication.Application.Abstractions;
using ApiApplication.Application.Proxies.Abstractions;
using ProtoDefinitions;
using System.Threading.Tasks;

namespace ApiApplication.Application.Proxies
{
    public class CachedMovieApiProxyDecorator : IExternalMovieApiProxy
    {
        private readonly IExternalMovieApiProxy _decoratedApiProxy;
        private readonly ICacheService _cacheService;


        public CachedMovieApiProxyDecorator(IExternalMovieApiProxy decoratedApiProxy, ICacheService cacheService)
        {
            _decoratedApiProxy = decoratedApiProxy;
            _cacheService = cacheService;
        }

        public async Task<showListResponse> GetAllAsync()
        {
            var cacheKey = "AllMovies";
            var cachedData = await _cacheService.GetAsync<showListResponse>(cacheKey);
            if (cachedData != null)
            {
                return cachedData;
            }

            var response = await _decoratedApiProxy.GetAllAsync();

            if (response != null)
            {
                await _cacheService.SetAsync(cacheKey, response);
            }

            return response;
        }

        public async Task<showResponse> GetByIdAsync(string id)
        {
            // use constant -milan jovanovic
            var cacheKey = $"MovieDetails-{id}";
            var cachedData = await _cacheService.GetAsync<showResponse>(cacheKey);
            if (cachedData != null)
            {
                return cachedData;
            }

            var response = await _decoratedApiProxy.GetByIdAsync(id);

            if (response != null)
            {
                await _cacheService.SetAsync(cacheKey, response);
            }

            return response;
        }
    }
}
