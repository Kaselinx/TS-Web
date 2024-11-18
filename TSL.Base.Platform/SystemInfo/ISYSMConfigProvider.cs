

namespace TSL.Base.Platform.SystemInfo
{
    /// <summary>
    /// 系統資料註記
    /// </summary>
    public interface ISYSMConfigProvider
    {
        /// <summary>
        /// Query all
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<SYSMCONFIG>> QueryAll();

    }
}
