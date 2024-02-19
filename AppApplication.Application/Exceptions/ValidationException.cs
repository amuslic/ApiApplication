namespace AppApplication.Application.Exceptions
{
    public class ValidationException : BaseException
    {
        public ValidationException(int statusCode, string? message) : base(statusCode, message ?? string.Empty)
        {
        }
    }
}
