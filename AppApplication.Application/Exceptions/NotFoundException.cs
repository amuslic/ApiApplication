namespace AppApplication.Application.Exceptions
{
    public class NotFoundException : BaseException
    {
        public NotFoundException(int statusCode, string? message) : base(statusCode, message ?? string.Empty)
        {
        }
    }
}
