﻿using System;

namespace Sample.Infrastructure.EventSourcing
{
    public interface IAggregate
    {
        Guid Id { get; }
        int Version { get; }
    }
}