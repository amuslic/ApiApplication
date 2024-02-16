using System.Net.Http;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using ProtoDefinitions;

namespace ApiApplication.Application
{
    public class ApiClientGrpc
    {
        private readonly MoviesApi.MoviesApiClient _client;

        public ApiClientGrpc()
        {
            var httpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            var channel = GrpcChannel.ForAddress("https://localhost:7443", new GrpcChannelOptions()
            {
                HttpHandler = httpHandler
            });

            _client = new MoviesApi.MoviesApiClient(channel);
        }

        public async Task<showListResponse> GetAllAsync()
        {
            var all = await _client.GetAllAsync(new Empty());
            all.Data.TryUnpack<showListResponse>(out var data);
            return data;
        }

        public async Task<showResponse> GetByIdAsync(string id)
        {
            var request = new IdRequest { Id = id };
            var movie = await _client.GetByIdAsync(request);
            movie.Data.TryUnpack<showResponse>(out var data);
            return data;
        }
    }
}
