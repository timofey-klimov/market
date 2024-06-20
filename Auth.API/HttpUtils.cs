namespace Auth.API
{
    public static class HttpUtils
    {
        public static string GetIpAddress(this HttpContext httpContext)
        {
            return string.IsNullOrEmpty(httpContext?.Connection?.RemoteIpAddress?.ToString())
                ? "unknown"
                : httpContext.Connection.RemoteIpAddress.ToString();
        }
    }
}
