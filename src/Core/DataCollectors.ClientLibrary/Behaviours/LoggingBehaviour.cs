using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using DataCollectors.ClientLibrary.Extensions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DataCollectors.ClientLibrary.Behaviours;

[ExcludeFromCodeCoverage]
public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILoggerFactory _loggerFactory;

    public LoggingBehaviour(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var logger = _loggerFactory.CreateLogger(typeof(TRequest).FullName!);

        try
        {
            logger.LogInformation("*** Executing ***");

            var stopwatch = Stopwatch.StartNew();
            var response = await next();
            stopwatch.Stop();

            logger.LogInformation("*** Completed - Elapsed: {ms} ***", stopwatch.Elapsed.ToReadableString());
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            throw;
        }
    }
}