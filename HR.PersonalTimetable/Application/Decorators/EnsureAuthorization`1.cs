﻿using Developist.Core.Cqrs;
using Developist.Core.Cqrs.Commands;
using Developist.Core.Persistence;
using Developist.Core.Persistence.Entities;
using Developist.Core.Utilities;

using HR.PersonalTimetable.Application.Exceptions;
using HR.PersonalTimetable.Application.Models;
using HR.PersonalTimetable.Application.Services;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace HR.PersonalTimetable.Application.Decorators
{
    public class EnsureAuthorization<TCommand> : IPrioritizable, ICommandHandlerWrapper<TCommand>
        where TCommand : ICommand
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IUnitOfWork unitOfWork;
        private readonly IClock clock;
        private readonly AppSettings appSettings;

        public EnsureAuthorization(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork, IOptions<AppSettings> appSettings, IClock clock)
        {
            this.httpContextAccessor = Ensure.Argument.NotNull(() => httpContextAccessor);
            this.unitOfWork = Ensure.Argument.NotNull(() => unitOfWork);
            this.appSettings = Ensure.Argument.NotNull(() => appSettings).Value;
            this.clock = Ensure.Argument.NotNull(() => clock);
        }

        public sbyte Priority => Priorities.Higher;

        public async Task HandleAsync(TCommand command, HandlerDelegate next, CancellationToken cancellationToken)
        {
            const BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            Integration integration = null;

            var property = command.GetType().GetProperty(nameof(Authorization), bindingAttr);
            if (property is not null && property.PropertyType == typeof(Authorization) && property.GetValue(command) is null)
            {
                var authorization = new Authorization
                {
                    Timestamp = GetTimestamp(),
                    UserName = GetAuthorization()
                };

                integration = await GetIntegrationAsync(cancellationToken).ConfigureAwait(false);
                authorization.SigningKey = integration.CurrentSigningKey;

                property.SetValue(command, authorization);
            }

            property = command.GetType().GetProperty(nameof(Integration), bindingAttr);
            if (property is not null && property.PropertyType == typeof(Integration) && property.GetValue(command) is null)
            {
                property.SetValue(command, integration);
            }

            await next().ConfigureAwait(false);
        }

        private int GetTimestamp()
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext.Request.Headers.TryGetValue("X-HR-Timestamp", out var timestamp))
            {
                if (int.TryParse(timestamp, out var unixTimeSeconds))
                {
                    var utcDate = DateTimeOffset.FromUnixTimeSeconds(unixTimeSeconds);

                    var clockSkew = Math.Floor(Math.Abs(clock.UtcNow.Subtract(utcDate).TotalSeconds));
                    if (clockSkew <= appSettings.ClockSkewToleranceInSeconds)
                    {
                        return unixTimeSeconds;
                    }
                    throw new UnauthorizedException("Clock skew between client and server is outside of tolerance.");
                }
                throw new BadRequestException("Invalid timestamp value provided.");
            }
            throw new BadRequestException("Required header \"X-HR-Timestamp\" was not present.");
        }

        private string GetAuthorization()
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext.Request.Headers.TryGetValue("X-HR-Authorization", out var authorization))
            {
                return authorization.ToString();
            }
            throw new BadRequestException("Required header \"X-HR-Authorization\" was not present.");
        }

        private async Task<Integration> GetIntegrationAsync(CancellationToken cancellationToken)
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext.Request.Headers.TryGetValue("X-HR-Integration", out var integrationName))
            {
                var integration = (await unitOfWork.Repository<Integration>().FindAsync(
                    integration => integration.Name == integrationName.ToString(),
                    includePaths => includePaths.Include(integration => integration.SigningKeys),
                    cancellationToken).ConfigureAwait(false)).SingleOrDefault() ?? throw new NotFoundException($"No integration with name \"{integrationName}\" found.");

                if (integration.SigningKeys.Any())
                {
                    return integration;
                }

                throw new NotFoundException($"No signing key found for integration with name \"{integration.Name}\".");
            }
            throw new BadRequestException("Required header \"X-HR-Integration\" was not present.");
        }
    }
}