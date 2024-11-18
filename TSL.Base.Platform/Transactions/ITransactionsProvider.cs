using Dapper;
using System.Collections.Concurrent;

namespace TSL.Base.Platform.Transactions
{
    /// <summary>
    /// ITransactionsProvider
    /// </summary>
    public interface ITransactionsProvider
    {
        /// <summary>
        /// 設定 traceId
        /// </summary>
        /// <param name="traceId">traceId</param>
        void SetTraceId(string traceId);

        #region Query

        /// <summary>
        /// ExecuteTransactionsAsync
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<int> ExecuteTransactionsAsync();

        #endregion

        #region Insert, Update, Delete

        /// <summary>
        /// DeleteRelatedTransactionsAsync
        /// </summary>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<int> DeleteRelatedTransactionsAsync();

        #endregion

        /// <summary>
        /// 是否存在於白名單
        /// </summary>
        /// <param name="tableName">資料表名稱</param>
        /// <returns></returns>
        bool ContainTableList(string tableName);

        /// <summary>
        /// TransactionId
        /// </summary>
        Guid TransactionId { get; }

        /// <summary>
        /// LogTraceId
        /// </summary>
        string LogTraceId { get; }

        /// <summary>
        /// 取得系統  Computed Columns 欄位清單
        /// </summary>
        ConcurrentDictionary<string, HashSet<string>> TableComputedColumns { get; }

        /// <summary>
        /// 資料快照 for Delete
        /// </summary>
        /// <param name="tableName">資料表名稱</param>
        /// <param name="keyColumnName">查詢欄位名稱</param>
        /// <param name="ids">資料 id</param>
        /// <returns></returns>
        Task DeleteWithSnapshot(string tableName, string keyColumnName, params string[] ids);

        /// <summary>
        /// 資料快照 for Update
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityToUpdate">entityToUpdate</param>
        /// <returns></returns>
        Task<bool> UpdateWithSnapshot<T>(T entityToUpdate) where T : class;

        /// <summary>
        /// 合併 Transaction script For Delete
        /// </summary>
        /// <param name="tableName">tableName</param>
        /// <param name="snapshotSqlStatement">snapshot sql statement</param>
        /// <param name="parameters">parameters</param>
        /// <returns></returns>
        string MergeTransactionScriptForDelete(string tableName, string snapshotSqlStatement, DynamicParameters parameters);

        /// <summary>
        /// 合併 Transaction script For Update
        /// </summary>
        /// <param name="tableName">tableName</param>
        /// <param name="snapshotSqlStatement">snapshot sql statement</param>
        /// <param name="parameters">parameters</param>
        /// <returns></returns>
        string MergeTransactionScriptForUpdate(string tableName, string snapshotSqlStatement, DynamicParameters parameters);
    }
}