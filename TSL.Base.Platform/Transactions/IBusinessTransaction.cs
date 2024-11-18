

namespace TSL.Base.Platform.Transactions
{
    /// <summary>
    /// IBusinessTransaction
    /// </summary>
    public interface IBusinessTransaction
    {
        /// <summary>
        /// 設定 LogTraceId
        /// </summary>
        /// <param name="traceId">traceId</param>
        void SetLogTraceId(string traceId);

        /// <summary>
        /// CompleteTransactions
        /// </summary>
        /// <returns></returns>
        Task CompleteTransactionsAsync();

        /// <summary>
        /// 須執行 rollback 操作
        /// </summary>
        /// <param name="methodBase"></param>
        /// <param name="message">訊息備註</param>
        void SwitchNeedExcuteRollback(System.Reflection.MethodBase methodBase, string message = null);
    }
}
