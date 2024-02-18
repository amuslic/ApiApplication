using ApiApplication.Application.Abstractions;
using System;

namespace ApiApplication.Application
{
    public class SystemTime : ISystemTime
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
