using AppApplication.Application.Abstractions;
using System;

namespace AppApplication.Application
{
    public class SystemTime : ISystemTime
    {
        public DateTime Now => DateTime.Now;
    }
}
