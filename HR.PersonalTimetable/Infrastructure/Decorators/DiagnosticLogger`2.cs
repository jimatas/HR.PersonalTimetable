using HR.Common.Cqrs;
using HR.Common.Cqrs.Queries;
using HR.Common.Utilities;

using Microsoft.Extensions.Logging;

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalTimetable.Infrastructure.Decorators
{
    public class DiagnosticLogger<TQuery, TResult> : IPrioritizable, IQueryHandlerWrapper<TQuery, TResult>
        where TQuery : IQuery<TResult>
    {
        private static JsonSerializerOptions jsonOptions;
        private readonly ILogger logger;

        public DiagnosticLogger(ILogger<DiagnosticLogger<TQuery, TResult>> logger)
        {
            this.logger = Ensure.Argument.NotNull(() => logger);
        }

        public sbyte Priority => Priorities.VeryHigh;

        public async Task<TResult> HandleAsync(TQuery query, HandlerDelegate<TResult> next, CancellationToken cancellationToken)
        {
            logger.LogDebug("Executing {QueryType} query {QueryDetails}", query.GetType().Name, JsonSerialize(query));

            var stopwatch = Stopwatch.StartNew();
            var result = await next();
            stopwatch.Stop();

            logger.LogDebug("Executed {QueryType} query in {Milliseconds} ms.", query.GetType().Name, stopwatch.ElapsedMilliseconds);

            return result;
        }

        private static object JsonSerialize(object value)
        {
            return new DeferredToString(() => JsonSerializer.Serialize(value, CreateDefaultJsonOptions()));

            static JsonSerializerOptions CreateDefaultJsonOptions() => jsonOptions ??= new()
            {
                WriteIndented = false,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }
    }
}
