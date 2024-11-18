using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSL.Base.Platform.Services
{
    /// <summary>
    /// Get context information for logging
    /// </summary>
    public class HttpContextInfoService : IHttpContextInfoService
    {
        public string RemoteIpAddress { get; private set; } = string.Empty;
        public string TraceId { get; private set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpContextInfoService"/> class.
        /// </summary>
        public HttpContextInfoService()
        {

        }

        public void SetHttpContext(HttpContext httpContext)
        {
            this.RemoteIpAddress = httpContext.Connection?.RemoteIpAddress?.ToString() ?? string.Empty;
            this.TraceId = httpContext.TraceIdentifier;
        }
    }
}

