using HR.Common.Cqrs;
using HR.Common.Cqrs.Commands;
using HR.Common.Utilities;

using Microsoft.Extensions.Logging;

using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalTimetable.Infrastructure.Decorators
{
    public class DiagnosticLogger<TCommand> : IPrioritizable, ICommandHandlerWrapper<TCommand>
        where TCommand : ICommand
    {
        private static JsonSerializerOptions jsonOptions;
        private readonly ILogger logger;

        public DiagnosticLogger(ILogger<DiagnosticLogger<TCommand>> logger)
        {
            this.logger = Ensure.Argument.NotNull(() => logger);
        }

        public sbyte Priority => Priorities.VeryHigh;

        public async Task HandleAsync(TCommand command, HandlerDelegate next, CancellationToken cancellationToken)
        {
            logger.LogDebug("Executing {CommandType} command {CommandDetails}", command.GetType().Name, JsonSerialize(command));

            var stopwatch = Stopwatch.StartNew();
            await next();
            stopwatch.Stop();

            logger.LogDebug("Executed {CommandType} command in {Milliseconds} ms.", command.GetType().Name, stopwatch.ElapsedMilliseconds);
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
