﻿// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.DinnerFiesta.Modules.Core.Application;

using System.Threading;
using System.Threading.Tasks;
using BridgingIT.DevKit.Application.Commands;
using BridgingIT.DevKit.Common;
using BridgingIT.DevKit.Domain;
using BridgingIT.DevKit.Domain.Repositories;
using BridgingIT.DevKit.Examples.DinnerFiesta.Modules.Core.Domain;
using Microsoft.Extensions.Logging;

public class HostUpdateCommandHandler(
    ILoggerFactory loggerFactory,
    IGenericRepository<Host> repository) : CommandHandlerBase<HostUpdateCommand, Result<Host>>(loggerFactory)
{
    public override async Task<CommandResponse<Result<Host>>> Process(HostUpdateCommand command, CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(command, nameof(command));

        var hostResult = await repository.FindOneResultAsync(HostId.Create(command.Id), cancellationToken: cancellationToken);
        var host = hostResult.Value;
        if (hostResult.IsFailure)
        {
            return CommandResponse.For(hostResult);
        }

        Check.Throw(new IBusinessRule[] { /*no rules yet*/ });

        host.ChangeName(command.FirstName, command.LastName)
            .ChangeProfileImage(command.ImageUrl is not null ? new Uri(command.ImageUrl) : null);

        await repository.UpdateAsync(host, cancellationToken);

        return CommandResponse.Success(host);
    }
}