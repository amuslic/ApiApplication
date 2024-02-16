using System;

namespace ApiApplication.Application.Exceptions
{
    public class BaseException : Exception
    {
        public string Message { get; set; }
        public int StatusCode { get; set; }

        public BaseException(int statusCode, string message )
        {
            Message = message;
            StatusCode = statusCode;
        }
    }
}
