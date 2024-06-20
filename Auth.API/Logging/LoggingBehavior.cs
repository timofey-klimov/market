using MediatR;
using Newtonsoft.Json;

namespace Auth.API.Logging
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest: notnull
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = request.GetType().Name;
            _logger.LogInformation("Start handle {Request} with {Body}", requestName, JsonConvert.SerializeObject(request));
            var result = await next();
            _logger.LogInformation("End handle {Request} with {Result}", requestName, JsonConvert.SerializeObject(result));
            return result;
           
        }
    }
}
