using Dapper;
using TSL.Base.Platform.DataAccess;
using TSL.Base.Platform.lnversionOfControl;
using TSL.Base.Platform.Log;
using TSL.Common.Model.DataAccessLayer.TZLog;
using TSL.TAAA.DataAccessLayer.TOTP;

namespace TSL.TAAA.DataAccessLayer.TZLog
{
    [RegisterIOC]
    public class TZLogRecordProvider : ITZLogRecordProvider
    {
        private readonly DataAccessService<TZLogDataAccessOption> _tZLogDBAccessService;
        private readonly ILog<TZLogRecordProvider>? _logger;


        /// <summary>
        /// constr
        /// </summary>
        /// <param name="tZLogDBAccessService"></param>
        /// <param name="logger"></param>
        public TZLogRecordProvider(DataAccessService<TZLogDataAccessOption> tZLogDBAccessService, ILog<TZLogRecordProvider>? logger)
        {
            _tZLogDBAccessService = tZLogDBAccessService;
            _logger = logger;
        }


        /// <summary>
        /// Write log to TZLOG table
        /// </summary>
        /// <param name="tzlog">tzlog table</param>
        /// <returns></returns>
        public async Task<bool> InsertTZLogAsync(TZLogModel tzlog)
        {
            string sql = @"INSERT INTO TZLOG(USER_ID, LOG_DT, STAT, SRCE_IP, AP_CD, DATA_TYP, CNTS, APFN, ACT, OBJ_TYP, ACCS_ID, SQL_CMD, NOTE1, NOTE2, NOTE3, BSR_CD) 
                                  Values(@sUSER_ID,@sLOG_DT,@sSTAT,@sSRCE_IP,@sAP_CD,@sDATA_TYP,@iCNTS,@sAPFN,@sACT,@sOBJ_TYP,@sACCS_ID,@sSQL_CMD,@sNOTE1,@sNOTE2,@sNOTE3,@sBSR_CD);
                                  Select @@ROWCOUNT AS Result;";
            DynamicParameters param = new DynamicParameters();
            param.Add("sUSER_ID", tzlog.USER_ID);
            param.Add("sLOG_DT", tzlog.LOG_DT);
            param.Add("sSTAT", tzlog.STAT);
            param.Add("sSRCE_IP", tzlog.SRCE_IP);
            param.Add("sAP_CD", tzlog.AP_CD);
            param.Add("sDATA_TYP", tzlog.DATA_TYP);
            param.Add("iCNTS", tzlog.CNTS);
            param.Add("sAPFN", tzlog.APFN);
            param.Add("sACT", tzlog.ACT);
            param.Add("sOBJ_TYP", tzlog.OBJ_TYP);
            param.Add("sACCS_ID", tzlog.ACCS_ID);
            param.Add("sSQL_CMD", tzlog.SQL_CMD);
            param.Add("sNOTE1", tzlog.NOTE1);
            param.Add("sNOTE2", tzlog.NOTE2);
            param.Add("sNOTE3", tzlog.NOTE3);
            param.Add("sBSR_CD", tzlog.BSR_CD);


            return await _tZLogDBAccessService.ExecuteNonQueryAsync(sql, param, false, System.Data.CommandType.Text) > 0 ? true : false;
        }
    }
}
