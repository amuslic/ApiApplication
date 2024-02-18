using System;

namespace ApiApplication.Application.Abstractions
{
    public interface ISystemTime
    {
        DateTimeOffset UtcNow { get; }
    }
}
