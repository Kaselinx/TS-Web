using TSL.Base.Platform.lnversionOfControl;

namespace TSL.Base.Platform.Utilities
{
    public class ServiceResult
    {
        /// <summary>
        /// 成功 (200000)
        /// </summary>
        public static readonly int SuccessCode = 200000;

        /// <summary>
        /// 非預期 Exception (900000)
        /// </summary>
        public static readonly int ExceptionErrorCode = 900000;

        /// <summary>
        /// 非預期 Faild (400000)
        /// </summary>
        public static readonly int FaildOfErrorCode = 400000;

        /// <summary>
        /// Service處理結果包裝物件
        /// </summary>
        public ServiceResult()
        {
        }

        /// <summary>
        /// Service處理結果包裝物件
        /// </summary>
        /// <param name="isOk">是否成功</param>
        public ServiceResult(bool isOk)
        {
            this.IsOk = isOk;
            this.Message = string.Empty;
        }

        /// <summary>
        /// Service處理結果包裝物件
        /// </summary>
        /// <param name="isOk">是否成功</param>
        /// <param name="message">訊息</param>
        public ServiceResult(bool isOk, string message)
        {
            this.IsOk = isOk;
            this.Message = message;
        }

        /// <summary>
        /// Service處理結果包裝物件.
        /// </summary>
        /// <param name="code">訊息代碼.</param>
        /// <param name="message">訊息.</param>
        public ServiceResult(string message, int code)
        {
            this.Message = message;
            this.Code = code;
        }

        /// <summary>
        /// Service處理結果包裝物件
        /// </summary>
        /// <param name="isOk">是否成功</param>
        /// <param name="code">訊息代碼</param>
        public ServiceResult(bool isOk, int code)
        {
            this.IsOk = isOk;
            this.Code = code;
        }

        /// <summary>
        /// Service處理結果包裝物件
        /// </summary>
        /// <param name="isOk">是否成功</param>
        /// <param name="code">訊息代碼</param>
        /// <param name="message">訊息</param>
        public ServiceResult(bool isOk, int code, string message)
        {
            this.IsOk = isOk;
            this.Code = code;
            this.Message = message;
        }

        /// <summary>
        /// Service處理結果包裝物件
        /// </summary>
        /// <param name="isOk">是否成功</param>
        /// <param name="code">訊息代碼</param>
        /// <param name="ex">例外物件</param>
        public ServiceResult(bool isOk, int code, Exception ex)
        {
            this.IsOk = isOk;
            this.Code = code;
            this.Exception = ex;
        }

        /// <summary>
        /// Service處理結果包裝物件
        /// </summary>
        /// <param name="isOk">是否成功</param>
        /// <param name="code">訊息代碼</param>
        /// <param name="message">訊息</param>
        /// <param name="ex">例外物件</param>
        public ServiceResult(bool isOk, int code, string message, Exception ex)
        {
            this.IsOk = isOk;
            this.Code = code;
            this.Message = message;
            this.Exception = ex;
        }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsOk { get; set; }

        /// <summary>
        /// 訊息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Code
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// Exception
        /// </summary>
        public Exception Exception { get; set; }
    }

    /// <summary>
    /// 泛型Service處理結果包裝物件
    /// </summary>
    /// <typeparam name="T">任意類別</typeparam>
    public class ServiceResult<T> : ServiceResult
    {
        /// <summary>
        /// Service處理結果包裝物件
        /// </summary>
        public ServiceResult()
        {
            Data = default!;
        }

        /// <summary>
        /// Service處理結果包裝物件
        /// </summary>
        /// <param name="isOk">是否成功</param>
        /// <param name="data">回傳資料物件</param>
        public ServiceResult(bool isOk, T data)
            : base(isOk)
        {
            this.Data = data;
        }

        /// <summary>
        /// Service處理結果包裝物件
        /// </summary>
        /// <param name="isOk">是否成功</param>
        /// <param name="message">訊息</param>
        public ServiceResult(bool isOk, string message)
            : base(isOk, message)
        {
            this.Data = default(T);
        }

        /// <summary>
        /// Service處理結果包裝物件
        /// </summary>
        /// <param name="isOk">是否成功</param>
        /// <param name="message">訊息</param>
        /// <param name="data">回傳資料物件</param>
        public ServiceResult(bool isOk, string message, T data)
            : base(isOk, message)
        {
            this.Data = data;
        }

        /// <summary>
        /// Service處理結果包裝物件
        /// </summary>
        /// <param name="isOk">是否成功</param>
        /// <param name="code">訊息代碼</param>
        /// <param name="data">回傳資料物件</param>
        public ServiceResult(bool isOk, int code, T data)
            : base(isOk, code)
        {
            this.Data = data;
        }

        /// <summary>
        /// Service處理結果包裝物件
        /// </summary>
        /// <param name="isOk">是否成功</param>
        /// <param name="message">訊息</param>
        /// <param name="data">回傳資料物件</param>
        /// <param name="rollback">是否需要 rollback</param>
        /// <param name="methodBase">methodBase 記錄用</param>
        public ServiceResult(bool isOk, string message, T data, bool rollback, System.Reflection.MethodBase methodBase)
            : base(isOk, message)
        {
            this.Data = data;

            if (rollback)
            {
                var businessTransaction = IOCContainerService.Container.GetService<Transactions.IBusinessTransaction>();
                businessTransaction.SwitchNeedExcuteRollback(methodBase, message);
            }
        }



        /// <summary>
        /// Service處理結果包裝物件
        /// </summary>
        /// <param name="isOk">是否成功</param>
        /// <param name="code">訊息代碼</param>
        /// <param name="message">訊息</param>
        /// <param name="data">回傳資料物件</param>
        public ServiceResult(bool isOk, int code, string message, T data)
            : base(isOk, code, message)
        {
            this.Data = data;
        }

        /// <summary>
        /// Service處理結果包裝物件.
        /// </summary>
        /// <param name="isOk">是否成功</param>
        /// <param name="code">訊息代碼</param>
        /// <param name="message">訊息</param>
        /// <param name="data">回傳資料物件</param>
        public ServiceResult(int code, string message, T data)
        {
            this.Code = code;
            this.Message = message;
            this.Data = data;
        }

        /// <summary>
        /// Service處理結果包裝物件
        /// </summary>
        /// <param name="isOk">是否成功</param>
        /// <param name="code">訊息代碼</param>
        /// <param name="ex">例外物件</param>
        public ServiceResult(bool isOk, int code, Exception ex)
            : base(isOk, code)
        {
            this.Exception = ex;
        }

        /// <summary>
        /// Service處理結果包裝物件
        /// </summary>
        /// <param name="isOk">是否成功</param>
        /// <param name="code">訊息代碼</param>
        /// <param name="message">訊息</param>
        /// <param name="ex">例外物件</param>
        public ServiceResult(bool isOk, int code, string message, Exception ex)
            : base(isOk, code, message, ex)
        {
        }

        /// <summary>
        /// Service處理結果包裝物件
        /// </summary>
        /// <param name="isOk">是否成功</param>
        /// <param name="message">訊息</param>
        /// <param name="code"> Code </param>
        /// <param name="data"> 回傳泛行物件 </param>
        /// <param name="ex">例外物件</param>
        public ServiceResult(bool isOk, string message, int code, T data, Exception ex)
            : base(isOk, message)
        {
            this.Code = code;
            this.Data = data;
            this.Exception = ex;
        }

        /// <summary>
        /// 回傳資料物件
        /// </summary>
        public T Data { get; set; }
    }
}
