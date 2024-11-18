
using Microsoft.AspNetCore.Http;
using System.Text;

namespace TSL.Base.Platform.Utilities
{
    /// <summary>
    /// HttpRequestHelper
    /// </summary>
    public static class HttpRequestHelper
    {
        /// <summary>
        /// IsAjaxRequest
        /// </summary>
        /// <param name="request">request</param>
        /// <returns></returns>
        public static bool IsAjaxRequest(HttpRequest request)
        {
            if (!string.Equals(request.Query["X-Requested-With"], "XMLHttpRequest", StringComparison.Ordinal))
            {
                return string.Equals(request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.Ordinal);
            }

            return true;
        }

        /// <summary>
        /// IsCors
        /// </summary>
        /// <param name="request">request</param>
        /// <returns></returns>
        public static bool IsCors(HttpRequest request)
        {
            Microsoft.Extensions.Primitives.StringValues useCors = request.Headers["HisCors"];
            return useCors.DefaultIfEmpty(null).FirstOrDefault() == "true";
        }

        /// <summary>
        /// FormatRequest
        /// </summary>
        /// <param name="request">request</param>
        /// <returns></returns>
        public static async Task<string> FormatRequest(HttpRequest request)
        {
            // todo .net core 3.0 改使用這個 EnableBuffering
            // request.EnableRewind();
            request.EnableBuffering();

            string bodyAsText = await new StreamReader(request.Body).ReadToEndAsync().ConfigureAwait(false);

            request.Body.Seek(0, SeekOrigin.Begin);

            return $"{request.Method}: {bodyAsText}";
        }

        /// <summary>
        /// FormatResponse
        /// </summary>
        /// <param name="response">response</param>
        /// <returns></returns>
        public static async Task<string> FormatResponse(HttpResponse response)
        {
            // We need to read the response stream from the beginning...
            response.Body.Seek(0, SeekOrigin.Begin);

            string contentEncoding = response.Headers.ContentEncoding.ToString();

            // ...and copy it into a string
            string text;
            if (string.Equals(contentEncoding, "gzip", StringComparison.OrdinalIgnoreCase))
            {
                var gzipStream = new System.IO.Compression.GZipStream(response.Body, System.IO.Compression.CompressionMode.Decompress);

                text = await new StreamReader(gzipStream).ReadToEndAsync().ConfigureAwait(false);
            }
            else
            {
                text = await new StreamReader(response.Body).ReadToEndAsync().ConfigureAwait(false);
            }

            // We need to reset the reader for the response so that the client can read it.
            response.Body.Seek(0, SeekOrigin.Begin);

            // Return the string for the response, including the status code (e.g. 200, 404, 401, etc.)
            return $"{response.StatusCode}: {text}";
        }

        /// <summary>
        /// GetRequestUrlWithQueryString
        /// </summary>
        /// <param name="request">request</param>
        /// <returns></returns>
        public static string GetRequestUrlWithQueryString(Microsoft.AspNetCore.Http.HttpRequest request)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(request.Scheme + "://");

            if (request.Host.HasValue)
            {
                sb.Append(request.Host.Value);
            }

            if (request.PathBase.HasValue)
            {
                sb.Append(request.PathBase.Value);
            }

            if (request.Path.HasValue)
            {
                sb.Append(request.Path.Value);
            }

            if (request.QueryString.HasValue)
            {
                sb.Append(request.QueryString.Value);
            }

            return sb.ToString();
        }
    }
}
