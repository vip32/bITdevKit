﻿// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.DinnerFiesta.Modules.Core.Domain;

using System.Threading;
using System.Threading.Tasks;
using BridgingIT.DevKit.Domain;

public class IsValidPasswordRule(string password) : IDomainRule
{
    private readonly string password = password;

    public string Message => "Not a valid password";

    public Task<bool> IsEnabledAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(true);

    public Task<bool> ApplyAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(!string.IsNullOrEmpty(this.password)); // TODO: implement
    }
}

public static partial class UserRules
{
    public static IDomainRule IsValidPassword(string password) => new IsValidPasswordRule(password);
}