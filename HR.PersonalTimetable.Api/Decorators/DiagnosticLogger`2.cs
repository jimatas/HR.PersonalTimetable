using Developist.Core.Cqrs;
using Developist.Core.Cqrs.Queries;
using Developist.Core.Utilities;

using HR.PersonalTimetable.Api.Infrastructure;

using Microsoft.Extensions.Logging;

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalTimetable.Api.Decorators
{
    public class DiagnosticLogger<TQuery, TResult> : IQueryHandlerWrapper<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        private static JsonSerializerOptions serializerOptions;
        private readonly ILogger logger;

        public DiagnosticLogger(ILogger<DiagnosticLogger<TQuery, TResult>> logger)
        {
            this.logger = Ensure.Argument.NotNull(() => logger);
        }

        public async Task<TResult> HandleAsync(TQuery query, HandlerDelegate<TResult> next, CancellationToken cancellationToken)
        {
            logger.LogDebug("Executing {QueryType} {QueryDetails}", query.GetType().Name, JsonSerialize(query));

            var stopwatch = Stopwatch.StartNew();
            var result = await next().ConfigureAwait(false);
            stopwatch.Stop();

            logger.LogDebug("Executed {QueryType} in {Milliseconds} ms.", query.GetType().Name, stopwatch.ElapsedMilliseconds);

            return result;
        }

        private static object JsonSerialize(object value)
        {
            return new DeferredToString(() => JsonSerializer.Serialize(value, CreateDefaultSerializerOptions()));

            static JsonSerializerOptions CreateDefaultSerializerOptions() => serializerOptions ??= new()
            {
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }
    }
}
