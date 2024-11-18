using System.Collections.Concurrent;
using System.Data.SqlClient;

namespace TSL.Base.Platform.SystemInfo
{
    /// <summary>
    /// IDBInfoService
    /// </summary>
    public interface IDBInfoService
    {
        /// <summary>
        /// 資料庫資料版本
        /// </summary>
        string DBDataVersion { get; }

        /// <summary>
        /// 取得跳過 後端 policy 驗證判斷
        /// </summary>
        bool SkipServersideBackendPolicyVerification { get; set; }

        /// <summary>
        /// 取得跳過 前端 policy 驗證判斷
        /// </summary>
        bool SkipServersideFrontendPolicyVerification { get; set; }

        /// <summary>
        /// 取得系統  Computed Columns 欄位清單
        /// </summary>
        ConcurrentDictionary<string, HashSet<string>> TableComputedColumns { get; }

        /// <summary>
        /// 取得資料庫連線ip
        /// </summary>
        /// <returns></returns>
        string GetDBIp();

        SqlConnectionStringBuilder GetHISSqlConnectionStringBuilder();
    }
}
