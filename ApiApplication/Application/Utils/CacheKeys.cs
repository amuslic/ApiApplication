using System;

namespace ApiApplication.Application.Utils
{
    public static class CacheKeys
    {
        public static Func<string, string> MemberById = (memberId) => $"member-{memberId}";
    }
}
