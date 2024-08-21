﻿// MIT-License
// Copyright BridgingIT GmbH - All Rights Reserved
// Use of this source code is governed by an MIT-style license that can be
// found in the LICENSE file at https://github.com/bridgingit/bitdevkit/license

namespace BridgingIT.DevKit.Examples.DinnerFiesta.Modules.Core.Domain;

using BridgingIT.DevKit.Domain;

public class MenuCreatedDomainEvent : DomainEventBase
{
    public MenuCreatedDomainEvent() { } // needed for outbox deserialization

    public MenuCreatedDomainEvent(Menu menu)
    {
        EnsureArg.IsNotNull(menu, nameof(menu));

        //this.MenuId = MenuId.Create(menu.Id.Value);
    }

    public MenuId MenuId { get; }
}