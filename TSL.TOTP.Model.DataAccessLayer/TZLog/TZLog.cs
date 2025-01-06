

namespace TSL.Common.Model.DataAccessLayer.TZLog
{

    /// <summary>
    /// TZlog data accesslayer
    /// </summary>
    public class TZLogModel
    { 
        public string? USER_ID { get; set; }
        public string? LOG_DT { get; set; }
        public string? STAT { get; set; }
        public string? SRCE_IP { get; set; }
        public string? AP_CD { get; set; }
        public string? DATA_TYP { get; set; }
        public int CNTS { get; set; }
        public string? APFN { get; set; }
        public string? ACT { get; set; }
        public string? OBJ_TYP { get; set; }
        public string? ACCS_ID { get; set; }
        public string SQL_CMD { get; set; }
        public string? SQL_ID { get; set;}
        public string? NOTE1 { get; set; }
        public string? NOTE2 { get; set; }
        public string? NOTE3 { get; set; }
        public string? NOTE4 { get; set; }
        public string? NOTE5 { get; set; }
        public string? BSR_CD { get; set; }

    }
}
