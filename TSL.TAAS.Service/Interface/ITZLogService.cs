using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TSL.Common.Model.DataAccessLayer.TZLog;

namespace TSL.TAAA.Service.Interface
{
    public interface ITZLogService
    {
        bool InsertTZLog(string sUSER_ID, string sLOG_DT, string sSTAT, string sSRCE_IP, string sAP_CD, string sDATA_TYP, int iCNTS, string sAPFN, string sACT, string sOBJ_TYP, string sACCS_ID, string sSQL_CMD, string sNOTE1, string sNOTE2, string sNOTE3, string sBSR_CD);

    }
}
