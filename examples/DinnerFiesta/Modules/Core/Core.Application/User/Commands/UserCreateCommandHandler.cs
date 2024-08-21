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

public class UserCreateCommandHandler(
    ILoggerFactory loggerFactory,
    IGenericRepository<User> repository)
    : CommandHandlerBase<UserCreateCommand, Result<User>>(loggerFactory)
{
    public override async Task<CommandResponse<Result<User>>> Process(UserCreateCommand command, CancellationToken cancellationToken)
    {
        EnsureArg.IsNotNull(command, nameof(command));

        var user = User.Create(
            command.FirstName,
            command.LastName,
            command.Email,
            command.Password);

        DomainRules.Apply(
        [
            UserRules.EmailMustBeUnique(repository, user),
        ]);

        await repository.InsertAsync(user, cancellationToken);

        return new CommandResponse<Result<User>>
        {
            Result = Result<User>.Success(user)
        };
    }
}