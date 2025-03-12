using TSL.Base.Platform.lnversionOfControl;
using TSL.Base.Platform.Log;
using TSL.Common.Model.DataAccessLayer.TZLog;
using TSL.TAAA.DataAccessLayer.TZLog;
using TSL.TAAA.Service.Interface;

namespace TSL.TAAA.Service.TZLogService
{
    [RegisterIOC]
    public class TZLogService : ITZLogService
    {
        private readonly ITZLogRecordProvider _iTZLogProvider;
        private readonly ILog<TZLogService> _logger;

        /// <summary>
        /// constr 
        /// </summary>
        /// <param name="iTZLogProvider">tzlog provider</param>
        /// <param name="logger">logger</param>
        public TZLogService(ITZLogRecordProvider iTZLogProvider, ILog<TZLogService> logger)
        {
            _iTZLogProvider = iTZLogProvider ?? throw new ArgumentNullException(nameof(iTZLogProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        /// <summary>
        /// insert tzlog
        /// </summary>
        /// <param name="sUSER_ID"></param>
        /// <param name="sLOG_DT"></param>
        /// <param name="sSTAT"></param>
        /// <param name="sSRCE_IP"></param>
        /// <param name="sAP_CD"></param>
        /// <param name="sDATA_TYP"></param>
        /// <param name="iCNTS"></param>
        /// <param name="sAPFN"></param>
        /// <param name="sACT"></param>
        /// <param name="sOBJ_TYP"></param>
        /// <param name="sACCS_ID"></param>
        /// <param name="sSQL_CMD"></param>
        /// <param name="sNOTE1"></param>
        /// <param name="sNOTE2"></param>
        /// <param name="sNOTE3"></param>
        /// <param name="sBSR_CD"></param>
        /// <returns></returns>
        public bool InsertTZLog(string sUSER_ID, string sLOG_DT, string sSTAT, string sSRCE_IP, string sAP_CD, string sDATA_TYP, int iCNTS, string sAPFN, string sACT, string sOBJ_TYP, string sACCS_ID, string sSQL_CMD, string sNOTE1, string sNOTE2, string sNOTE3, string sBSR_CD)
        {
            try
            {
                TZLogModel tzlog = new TZLogModel();
                tzlog.USER_ID = sUSER_ID;
                tzlog.LOG_DT = sLOG_DT;
                tzlog.STAT = sSTAT;
                tzlog.SRCE_IP = sSRCE_IP;
                tzlog.AP_CD = sAP_CD;
                tzlog.DATA_TYP = sDATA_TYP;
                tzlog.CNTS = iCNTS;
                tzlog.APFN = sAPFN;
                tzlog.ACT = sACT;
                tzlog.OBJ_TYP = sOBJ_TYP;
                tzlog.ACCS_ID = sACCS_ID;
                tzlog.SQL_CMD = sSQL_CMD;
                tzlog.NOTE1 = sNOTE1;
                tzlog.NOTE2 = sNOTE2;
                tzlog.NOTE3 = sNOTE3;
                tzlog.BSR_CD = sBSR_CD;

                bool result = _iTZLogProvider.InsertTZLogAsync(tzlog).GetAwaiter().GetResult();

                if (result)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex, new { });
                return false;
            }
        }
    }
}

