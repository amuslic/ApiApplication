using System;

namespace ApiApplication.Application.Exceptions
{
    public class BadRequestException: BaseException
    {
        public BadRequestException(int statusCode, string? message) : base(statusCode, message ?? string.Empty)
        {
        }
    }
}
