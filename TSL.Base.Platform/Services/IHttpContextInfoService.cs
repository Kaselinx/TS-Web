using Microsoft.AspNetCore.Http;


namespace TSL.Base.Platform.Services
{
    /// <summary>
    /// Get require information for logging
    /// </summary>
    public interface IHttpContextInfoService
    {

        string RemoteIpAddress { get; }

        string TraceId { get; }

        void SetHttpContext(HttpContext httpContext);
    }
}
