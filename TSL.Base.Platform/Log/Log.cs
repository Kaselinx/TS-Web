using Microsoft.AspNetCore.Http;
using NLog;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TSL.Base.Platform.Log
{
    /// <summary>
    /// Customize Log function
    /// </summary>
    /// <typeparam name="T">Log Class Type</typeparam>
    public class Log<T> : ILog<T>
    {
        //private readonly ILogger<T> _logger;
        private readonly Logger _logger;
        private readonly Services.IHttpContextInfoService _httpContextInfoService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        ///  constructor
        /// </summary>
        /// //public Log(ILogger<T> logger, Services.IHttpContextInfoService httpContextInfoService, IHttpContextAccessor httpContextAccessor)
        /// <param name="logger">logger object</param>
        /// <param name="httpContextInfoService"></param>
        public Log(Services.IHttpContextInfoService httpContextInfoService, IHttpContextAccessor httpContextAccessor)
        {
            _logger = LogManager.GetLogger("mainLog");
            _httpContextInfoService = httpContextInfoService;
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        ///  info level log
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="objArray">list of arguments</param>
        public void Info(string message, params object[] objArray)
        {
            _logger?.Info(CultureInfo.CurrentCulture, FormatMessageWithParams(message, objArray));
        }


        /// <summary>
        ///  errror level log
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="objArray">list of arguments</param>
        public void Error(string message, params object[] objArray)
        {
            _logger.Error(CultureInfo.CurrentCulture, FormatMessageWithParams(message, objArray));
        }

        /// <summary>
        ///  errror level log
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="args">list of arguments</param>
        public void Error(Exception ex, string message, params object[] args)
        {
            _logger.Error(ex, message, FormatMessageWithParams(message, args));
        }

        /// <summary>
        ///  Debug level log
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="objArray">list of arguments</param>
        public void Debug(string message, params object[] objArray)
        {
            _logger?.Debug(CultureInfo.CurrentCulture, FormatMessageWithParams(message, objArray));
        }

        /// <summary>
        ///  Warn level log
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="args">list of arguments</param>
        public void Warn(string message, params object[] args)
        {
            _logger?.Warn(CultureInfo.CurrentCulture, message, args);
        }

        /// <summary>
        ///  Trace level log
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="objArray">list of arguments</param>
        public void Trace(string message, params object[] objArray)
        {
            _logger.Trace(CultureInfo.CurrentCulture, FormatMessageWithParams(message, objArray));
        }

        /// <summary>
        /// fatal level log
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="exception">exception msg</param>
        public void Critical(string message, Exception exception)
        {
            _logger.Fatal(message, exception);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="forceLog"></param>
        /// <param name="objArray"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void Debug(string message, bool forceLog = false, params object[] objArray)
        {
            throw new NotImplementedException();
        }

        public void Trace(string message, bool forceLog = false, params object[] objArray)
        {
            throw new NotImplementedException();
        }

        public void Error(string message, Exception exception, params object[] objArray)
        {
            _logger.Error( message, exception, FormatMessageWithParams(message, objArray));
        }

        public void Info(string message, bool forceLog = false, params object[] objArray)
        {
            throw new NotImplementedException();
        }

        public void Warning(string message, Exception exception, params object[] objArray)
        {
            throw new NotImplementedException();
        }

        public void Warning(string message, params object[] objArray)
        {
            throw new NotImplementedException();
        }

        public void APILogTrace(string localIPAddress, string remoteIPAddress, string systemName, string vendorCode, string venderName, string certificate, string type, string requestUrl, string message, string? traceId = null)
        {
            Logger logger = LogManager.GetLogger("WebAPILog");

            // 建立 Log 事件資訊
            LogEventInfo logEvenInfo = new LogEventInfo(NLog.LogLevel.Trace, "WebAPILog", message);

            // 設定自定義的屬性值 (名稱需對應至 NLog.config 參數)
            // <parameter name="@ERROR_CREATOR" layout="${event-properties:item=ErrorCreator}"/>
            logEvenInfo.Properties["LocalIPAddress"] = localIPAddress;
            logEvenInfo.Properties["RemoteIPAddress"] = remoteIPAddress;
            logEvenInfo.Properties["SystemName"] = systemName;
            logEvenInfo.Properties["VendorCode"] = vendorCode;
            logEvenInfo.Properties["VenderName"] = venderName;
            logEvenInfo.Properties["Certificate"] = certificate;
            logEvenInfo.Properties["Type"] = type;
            logEvenInfo.Properties["RequestUrl"] = requestUrl;
            logEvenInfo.Properties["Message"] = message;
            logEvenInfo.Properties["TraceId"] = GetTraceId(traceId ?? string.Empty);

            logger.Log(logEvenInfo);
        }

        public void AddHttpLog(string userCacheKey, string remoteIPAddress, string type, string requestUrl, string message)
        {
            throw new NotImplementedException();
        }

        public Task HttpClientLogTrace(DateTime requestTime, DateTime responseTime, HttpRequestMessage request, HttpResponseMessage response, string traceId)
        {
            throw new NotImplementedException();
        }

        public Task HttpClientLogTraceForError(DateTime requestTime, HttpRequestMessage request, Exception ex, string traceId)
        {
            throw new NotImplementedException();
        }

        public void RabbitMQLog(long? elapsedMilliseconds, string message, params object[] objArray)
        {
            throw new NotImplementedException();
        }

        public IBaseLog<T> Extension => throw new NotImplementedException();

        public ICustomTraceId<T> ExtensionCustomTraceId => throw new NotImplementedException();


        #region private
        public string GetTraceId(string cusTraceId)
        {
            if (string.IsNullOrEmpty(cusTraceId))
            {
                return string.IsNullOrEmpty(_httpContextInfoService?.TraceId) ? _httpContextAccessor?.HttpContext?.TraceIdentifier : _httpContextInfoService?.TraceId;
            }
            else
            {
                return cusTraceId;
            }
        }


        /// <summary>
        /// pass log to require format
        /// </summary>
        /// <param name="message">messge</param>
        /// <param name="objArray">passing value</param>
        /// <returns></returns>
        private string FormatMessageWithParams(string message, params object[] objArray)
        {
            StringBuilder sb = new StringBuilder(message);
            _ = sb.Append(message);
            string json = JsonConvert.SerializeObject(objArray, Formatting.None);
            _ = sb.Append(json);

            return sb.ToString();
        }

        #endregion
    }
}

