using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

namespace ApiApplication.Application.Commands
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting handling {RequestName}", typeof(TRequest).Name);

            var stopwatch = Stopwatch.StartNew();

            var response = await next();

            stopwatch.Stop();

            _logger.LogInformation(
                "Finished handling {typeof(RequestName}. Time taken: {ElapsedMilliseconds} ms",
                typeof(TRequest).Name,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
    }
}
