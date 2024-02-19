using System;

namespace AppApplication.Application.Utils
{
    public static class CacheKeys
    {
        public static Func<string, string> MemberById = (memberId) => $"member-{memberId}";
    }
}
