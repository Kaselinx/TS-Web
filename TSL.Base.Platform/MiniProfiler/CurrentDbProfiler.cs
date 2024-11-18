using StackExchange.Profiling.Data;
using System.Data.Common;
using System.Data;


namespace TSL.Base.Platform.MiniProfiler
{
    /// <summary>
    /// MiniProfiler的DbProfiler
    /// </summary>
    public class CurrentDbProfiler : IDbProfiler
    {
        private Func<IDbProfiler> GetProfiler { get; }

        /// <summary>
        /// 建構子
        /// </summary>
        /// <param name="getProfiler"></param>
        public CurrentDbProfiler(Func<IDbProfiler> getProfiler)
        {
            GetProfiler = getProfiler;
        }

        /// <summary>
        /// 確認MiniProfiler是否存在，避免發生異常Null Reference Exception
        /// </summary>
        public bool IsActive => GetProfiler()?.IsActive ?? false;

        public void ExecuteFinish(IDbCommand profiledDbCommand, SqlExecuteType executeType, DbDataReader? reader)
        {
            GetProfiler()?.ExecuteFinish(profiledDbCommand, executeType, reader);
        }

        public void ExecuteStart(IDbCommand profiledDbCommand, SqlExecuteType executeType)
        {
            GetProfiler()?.ExecuteStart(profiledDbCommand, executeType);
        }

        public void OnError(IDbCommand profiledDbCommand, SqlExecuteType executeType, Exception exception)
        {
            GetProfiler()?.OnError(profiledDbCommand, executeType, exception);
        }

        public void ReaderFinish(IDataReader reader)
        {
            GetProfiler()?.ReaderFinish(reader);
        }
    }
}
