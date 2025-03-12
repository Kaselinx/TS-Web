

namespace TSL.Common.Model.DataAccessLayer.OTP
{
    public class AuthOTP_Request
    {
        public required string SystemID { get; set; }
        public required string SeqNo { get; set; }
        public required string OTP { get; set; }
    }
}
