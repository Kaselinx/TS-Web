using System.Net;
using TSL.Base.Platform.lnversionOfControl;
using TSL.Base.Platform.Log;
using TSL.Base.Platform.Utilities;

namespace TSL.TAAS.API.Helpers
{
    /// <summary>
    /// Web API Logging Middleware
    /// </summary>
    public class WebAPIRequestResponseLogMiddleware
    {
        private readonly RequestDelegate _next;
    
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="next">Request Delegate</param>
        /// <param name="iocContainer">iocContainer</param>
        public WebAPIRequestResponseLogMiddleware(RequestDelegate next, IOCContainer iocContainer)
        {
            _next = next;
        }

        /// <summary>
        /// Invoke
        /// </summary>
        /// <param name="context">context</param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            ILog<WebAPIRequestResponseLogMiddleware> logger = context.RequestServices.GetService(typeof(ILog<WebAPIRequestResponseLogMiddleware>)) as ILog<WebAPIRequestResponseLogMiddleware>;

            try
            {
                // First, get the incoming request
                var request = await HttpRequestHelper.FormatRequest(context.Request).ConfigureAwait(false);
                string? requestUrl = HttpRequestHelper.GetRequestUrlWithQueryString(context.Request);
                string? localIPAddress = context.Connection?.LocalIpAddress?.MapToIPv4().ToString() + ":" + context.Connection?.LocalPort;
                string? remoteIPAddress = context.Connection?.RemoteIpAddress?.MapToIPv4().ToString() + ":" + context.Connection?.RemotePort;
                string? certificate = context.Connection?.ClientCertificate?.ToString();
                string? traceid = string.Empty;

                traceid = logger?.GetTraceId(traceid);
                logger.APILogTrace(localIPAddress, remoteIPAddress, "SystemName", "VendorCode", "VendorName", certificate, APIType.Request.ToString(), requestUrl, request, traceid);
                


                // Copy a pointer to the original response body stream
                var originalBodyStream = context.Response.Body;

                // Create a new memory stream...
                using (var responseBody = new MemoryStream())
                {
                    // ...and use that for the temporary response body
                    context.Response.Body = responseBody;  

                    // Continue down the Middleware pipeline, eventually returning to this class
                    await _next(context).ConfigureAwait(false);

                    // Format the response from the server
                    var response = await HttpRequestHelper.FormatResponse(context.Response).ConfigureAwait(false);

                    if (requestUrl.IndexOf("/Common/HealthStatus", StringComparison.OrdinalIgnoreCase) == -1)
                    {
                        // _logger.Info(nameof(Invoke), "WebAPI Response : " + response);
                        logger.APILogTrace(localIPAddress, remoteIPAddress, "SystemName", "VendorCode", "VendorName", certificate, APIType.Response.ToString(), requestUrl, response, traceid);
                    }

                    // Copy the contents of the new memory stream (which contains the response) to the original stream, which is then returned to the client.
                    await responseBody.CopyToAsync(originalBodyStream).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                logger.Error(nameof(Invoke), ex);
            }
        }



        private enum APIType
        {
            Request,

            Response
        }
    }
}
