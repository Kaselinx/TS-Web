using TSL.Base.Platform.lnversionOfControl;

namespace TSL.Base.Platform.Log
{
    /// <summary>
    /// Logging the request and response of the HttpClient
    /// </summary>
    public class HttpClientLoggingHandler : DelegatingHandler
    {
        /// <summary>
        /// HeadContextTraceId head 名稱
        /// </summary>
        public const string HeadContextTraceId = "__request_trace_id";

        /// <summary>
        /// 增加 http context id
        /// </summary>
        /// <param name="httpclient">httpclient</param>
        /// <param name="contextTraceId">contextTraceId</param>
        public static void AddContextTraceId(HttpClient httpclient, string contextTraceId)
        {

            if (!httpclient.DefaultRequestHeaders.Contains(HeadContextTraceId))
            {
                httpclient.DefaultRequestHeaders.Add(HeadContextTraceId, contextTraceId);
            }
            else
            {
                var oldTraceId = httpclient.DefaultRequestHeaders.GetValues(HttpClientLoggingHandler.HeadContextTraceId).FirstOrDefault();

                // 若不同 trace id 則送入
                if (string.Equals(oldTraceId, contextTraceId, StringComparison.OrdinalIgnoreCase) == false)
                {
                    httpclient.DefaultRequestHeaders.Add(HeadContextTraceId, contextTraceId);
                }
            }
        }

        /// <summary>
        /// 取得 trace id
        /// </summary>
        /// <param name="request">request</param>
        /// <returns></returns>
        public static string GetHeadTraceId(HttpRequestMessage request)
        {
            string traceId = string.Empty;

            if (request.Headers.Contains(HttpClientLoggingHandler.HeadContextTraceId))
            {
                traceId = request.Headers.GetValues(HeadContextTraceId).FirstOrDefault();
            }

            return traceId;
        }

        private IOCContainer _container;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="container">The container.</param>
        public HttpClientLoggingHandler(IOCContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        /// <summary>
        /// Override Send Async for logging
        /// </summary>
        /// <param name="request">HttpRequestMessage</param>
        /// <param name="cancellationToken">CancellationToken</param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var logger = _container.GetService<ILog<HttpClientLoggingHandler>>();

            string traceId = GetHeadTraceId(request);
            var requestTime = DateTime.Now;
            try
            {
                logger.ExtensionCustomTraceId.Info(traceId, "Request : " + request.ToString());

                HttpResponseMessage response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

                logger.ExtensionCustomTraceId.Info(traceId, $"Response : " + response);
                DateTime responseTime = DateTime.Now;
                await logger.HttpClientLogTrace(requestTime, responseTime, request, response, traceId).ConfigureAwait(false);

                return response;
            }
            catch (Exception ex)
            {
                logger.ExtensionCustomTraceId.Error(traceId, nameof(SendAsync), ex.Message);
                await logger.HttpClientLogTraceForError(requestTime, request, ex, traceId).ConfigureAwait(false);
                throw;
            }

        }
    }
}
