using ApiApplication.Application.Proxies.Abstractions;
using Grpc.Core;
using Grpc.Net.Client;
using ProtoDefinitions;
using System.Threading.Tasks;
using System;
using System.Net.Http;

public class ExternalMovieApiProxy : IExternalMovieApiProxy
{
    private readonly MoviesApi.MoviesApiClient _client;

    public ExternalMovieApiProxy()
    {
         var httpHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        var channel =
            GrpcChannel.ForAddress("https://localhost:7443", new GrpcChannelOptions()
            {
                HttpHandler = httpHandler
            });

        _client = new MoviesApi.MoviesApiClient(channel);
    }

    private static Metadata GetDefaultHeaders()
    {
        // read from ioptions monitor configuration
        return new Metadata
        {
            { "X-Apikey", "68e5fbda-9ec9-4858-97b2-4a8349764c63" }
        };
    }

    public async Task<showListResponse> GetAllAsync()
    {
        try
        {
            var headers = GetDefaultHeaders();
            var all = await _client.GetAllAsync(new Empty(), new CallOptions(headers));
            if (all.Data.TryUnpack<showListResponse>(out var data))
            {
                return data;
            }

            throw new InvalidOperationException("Failed to unpack the showListResponse.");
        }
        catch (Exception ex)
        {
            // Log and handle exception
            throw;
        }
    }

    public async Task<showResponse> GetByIdAsync(string id)
    {
        try
        {
            var headers = GetDefaultHeaders();
            var request = new IdRequest { Id = id };
            var movie = await _client.GetByIdAsync(request, new CallOptions(headers));
            if (movie.Data.TryUnpack<showResponse>(out var data))
            {
                return data;
            }

            throw new InvalidOperationException("Failed to unpack the showResponse.");
        }
        catch (Exception ex)
        {
            // Log and handle exception
            throw;
        }
    }
}
