using Grpc.Core;
using Grpc.Net.Client;
using ProtoDefinitions;
using System.Threading.Tasks;
using System;
using System.Net.Http;
using ApiApplication.Application.Abstractions;
using ApiApplication.Application.Configuration;
using Microsoft.Extensions.Options;
using System.Runtime.InteropServices;
using System.Linq;
using ApiApplication.Application.Exceptions;
using Microsoft.AspNetCore.Http;

namespace ApiApplication.Application
{

    public class ExternalMovieApiProxy : IExternalMovieApiProxy
    {
        private readonly MoviesApi.MoviesApiClient _client;
        private readonly string _apiKey;

        public ExternalMovieApiProxy(IOptionsMonitor<ExternalMovieProviderConfiguration> movieProviderOptions)
        {
            var options = movieProviderOptions.CurrentValue;
            _apiKey = options.ApiKey;

            var httpHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            var channel = GrpcChannel.ForAddress(options.Url, new GrpcChannelOptions { HttpHandler = httpHandler });
            _client = new MoviesApi.MoviesApiClient(channel);
        }

        private Metadata GetDefaultHeaders()
        {
            return new Metadata
        {
            { "X-Apikey", _apiKey }
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
                throw new ExternalException("Failed to fetch showListResponse.");
            }
        }

        public async Task<showResponse> GetByIdAsync(string id)
        {
            var headers = GetDefaultHeaders();
            var request = new IdRequest { Id = id };
            var movie = await _client.GetByIdAsync(request, new CallOptions(headers));

            if (movie.Exceptions.Any())
            {
                // client doesnt have better exception handling, no status code
                if (movie.Exceptions.FirstOrDefault().Message.Contains("Not found"))
                {
                    throw new NotFoundException
                        (StatusCodes.Status404NotFound,
                        $"Movie with id {id} doesnt exist");
                }
            }


            if (movie.Data.TryUnpack<showResponse>(out var data))
            {
                return data;
            }

            throw new InvalidOperationException("Failed to unpack the showResponse.");
        }
    }
}
