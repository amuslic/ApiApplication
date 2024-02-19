using System;

namespace AppApplication.Application.Abstractions
{
    public interface ISystemTime
    {
        DateTime Now { get; }
    }
}
