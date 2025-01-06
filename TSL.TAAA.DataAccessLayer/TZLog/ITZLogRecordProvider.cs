
using TSL.Common.Model.DataAccessLayer.TZLog;

namespace TSL.TAAA.DataAccessLayer.TZLog
{
    public interface ITZLogRecordProvider
    {
        /// <summary>
        /// Insert to TOTP secret data table. 
        /// </summary>
        /// <param name="TZLog"></param>
        /// <returns></returns>
        Task<bool> InsertTZLogAsync(TZLogModel tzlog);
    }
}
