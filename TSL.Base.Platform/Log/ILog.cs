
namespace TSL.Base.Platform.Log
{

    public interface IBaseLog<T>
    {
        void Critical(string medicalNoteNo, long? encounterId, string message, Exception exception); // Add medicalNoteNo and encounterId

        void Error(string medicalNoteNo, long? encounterId, string message, Exception exception, params object[] objArray);

        void Error(string medicalNoteNo, long? encounterId, string message, params object[] objArray);

        void Warning(string medicalNoteNo, long? encounterId, string message, Exception exception, params object[] objArray);

        void Warning(string medicalNoteNo, long? encounterId, string message, params object[] objArray);

        void Info(string medicalNoteNo, long? encounterId, string message, params object[] objArray);

        void Debug(string medicalNoteNo, long? encounterId, string message, params object[] objArray);

    }

    public interface ICustomTraceId<T>
    {
        void Info(string traceId, string message, params object[] objArray);

        void Error(string traceId, string message, params object[] objArray);

        void Debug(string traceId, string message, params object[] objArray);

        void Warning(string traceId, string message, params object[] objArray);
    }

    /// <summary>
    /// Log Interface
    /// </summary>
    /// <typeparam name="T">Log Class</typeparam>
    public interface ILog<T>
    {
        IBaseLog<T> Extension { get; }

        ICustomTraceId<T> ExtensionCustomTraceId { get; }

        /// <summary>
        /// Critical Log
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="exception">exception</param>
        void Critical(string message, Exception exception);

        /// <summary>
        /// Debug Log
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="objArray">log parameter object</param>
        void Debug(string message, params object[] objArray);

        /// <summary>
        /// Debug Log
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="forceLog">force log regardless of log level (強制Log)</param>
        /// <param name="objArray">log parameter object</param>
        void Debug(string message, bool forceLog = false, params object[] objArray);

        /// <summary>
        /// Trace Log
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="objArray">log parameter object</param>
        void Trace(string message, params object[] objArray);

        /// <summary>
        /// Trace Log
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="forceLog">force log regardless of log level (強制Log)</param>
        /// <param name="objArray">log parameter object</param>
        void Trace(string message, bool forceLog = false, params object[] objArray);

        /// <summary>
        /// Error Log
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="exception">exception</param>
        /// <param name="objArray">log parameter object</param>
        void Error(string message, Exception exception, params object[] objArray);

        /// <summary>
        /// Error Log
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="objArray">log parameter object</param>
        void Error(string message, params object[] objArray);

        /// <summary>
        /// Information Log
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="objArray">log parameter object</param>
        void Info(string message, params object[] objArray);

        /// <summary>
        /// Information Log
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="forceLog">force log regardless of log level (強制Log)</param>
        /// <param name="objArray">log parameter object</param>
        void Info(string message, bool forceLog = false, params object[] objArray);

        /// <summary>
        /// Warning Log
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="exception">exception</param>
        /// <param name="objArray">log parameter object</param>
        void Warning(string message, Exception exception, params object[] objArray);

        /// <summary>
        /// Warning Log
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="objArray">log parameter object</param>
        void Warning(string message, params object[] objArray);

        /// <summary>
        /// Only For Web API Middleware Log
        /// </summary>
        /// <param name="localIPAddress">Http context localIPAddress</param>
        /// <param name="remoteIPAddress">Http context remoteIPAddress</param>
        /// <param name="systemName">External system name</param>
        /// <param name="vendorCode">Vendor Code</param>
        /// <param name="venderName">Vendor Name</param>
        /// <param name="certificate">Client certificate</param>
        /// <param name="type">API Type</param>
        /// <param name="message">message</param>
        /// <param name="objArray">log parameter object</param>
        void APILogTrace(string localIPAddress, string remoteIPAddress, string systemName, string vendorCode, string venderName, string certificate, string type, string requestUrl, string message, string traceid);


        /// <summary>
        /// 紀錄 http request / response
        /// </summary>
        /// <param name="userCacheKey">userCacheKey</param>
        /// <param name="remoteIPAddress">remoteIPAddress</param>
        /// <param name="type">type</param>
        /// <param name="requestUrl">requestUrl</param>
        /// <param name="message">message</param>
        void AddHttpLog(string userCacheKey, string remoteIPAddress, string type, string requestUrl, string message);

        /// <summary>
        /// Only For HttpClient Handler Log
        /// </summary>
        /// <param name="requestTime">The RequestTime of HttpClient</param>
        /// <param name="responseTime">The ResponseTime of HttpClient</param>
        /// <param name="request">HttpRequestMessage</param>
        /// <param name="response">HttpResponseMessage</param>
        /// <param name="traceId">contextTraceId</param>
        /// <returns></returns>
        Task HttpClientLogTrace(DateTime requestTime, DateTime responseTime, HttpRequestMessage request, HttpResponseMessage response, string traceId);

        /// <summary>
        /// Only For HttpClient Handler Log
        /// </summary>
        /// <param name="requestTime">The RequestTime of HttpClient</param>
        /// <param name="request">request</param>
        /// <param name="ex">Exception</param>
        /// <param name="traceId">contextTraceId</param>
        /// <returns></returns>
        Task HttpClientLogTraceForError(DateTime requestTime, HttpRequestMessage request, Exception ex, string traceId);

        /// <summary>
        /// RabbitMQLog
        /// </summary>
        /// <param name="elapsedMilliseconds">執行時間</param>
        /// <param name="message">message</param>
        /// <param name="objArray">log parameter object</param>
        void RabbitMQLog(long? elapsedMilliseconds, string message, params object[] objArray);

        /// <summary>
        /// get traceld
        /// </summary>
        /// <param name="traceId">get trace Id</param>
        /// <returns></returns>
        string GetTraceId(string traceId);


    }
}