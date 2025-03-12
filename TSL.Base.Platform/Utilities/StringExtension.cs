
using TSL.Common.Model.DataAccessLayer.OTP;

namespace TSL.Base.Platform.Utilities
{
    public static class StringExtension
    {
        //public StringExtension()
        //{
        //    //
        //    // TODO: 在這裡新增建構函式邏輯
        //    //

        //}

        //check data request is pass security issue 
        public static bool check_RequestData(OTP_Request OTP_Request)
        {
            bool bResult = true;
            try
            {
                if (OTP_Request.SystemID == null || (OTP_Request.Mail == null && OTP_Request.Tel_No == null))
                {
                    bResult = false;
                }
                else
                {

                    string _SystemID = SafeRequestStringWithLength(OTP_Request.SystemID, OTP_Request.SystemID.Length);
                    string _Mail = OTP_Request.Mail != null ? SafeRequestStringWithLength(OTP_Request.Mail, OTP_Request.Mail.Length) : OTP_Request.Mail;
                    string _Tel_No = OTP_Request.Tel_No != null ? SafeRequestStringWithLength(OTP_Request.Tel_No, OTP_Request.Tel_No.Length) : OTP_Request.Tel_No;
                    string _Effect_Second = SafeRequestStringWithLength(OTP_Request.Effect_Second.ToString(), OTP_Request.Effect_Second.ToString().Length);

                    if (OTP_Request.SystemID != _SystemID || OTP_Request.Mail != _Mail || OTP_Request.Tel_No != _Tel_No || OTP_Request.Effect_Second.ToString() != _Effect_Second)
                    {
                        bResult = false;
                    }
                }

            }
            catch (Exception ex)
            {
                bResult = false;
            }

            return bResult;
        }

        public static bool check_RequestData(AuthOTP_Request AuthOTP_Request)
        {
            bool bResult = true;
            try
            {
                string _SystemID = SafeRequestStringWithLength(AuthOTP_Request.SystemID, AuthOTP_Request.SystemID.Length);
                string _SeqNo = SafeRequestStringWithLength(AuthOTP_Request.SeqNo, AuthOTP_Request.SeqNo.Length);
                string _OTP = SafeRequestStringWithLength(AuthOTP_Request.OTP, AuthOTP_Request.OTP.Length);

                if (AuthOTP_Request.SystemID != _SystemID || AuthOTP_Request.SeqNo != _SeqNo || AuthOTP_Request.OTP != _OTP)
                {
                    bResult = false;
                }
            }
            catch (Exception ex)
            {
                bResult = false;
            }

            return bResult;
        }

        //filter data
        public static string SafeRequestStringWithLength(this string instance, int length)
        {
            if (string.IsNullOrWhiteSpace(instance)) return string.Empty;

            string returnstring = string.Empty;
            instance = instance.Replace("'", string.Empty).Replace("--", string.Empty);
            if (instance.Length == 0)
                returnstring = instance;
            else if (instance.Length <= length)
                returnstring = instance;
            else if (instance.Length > length)
                returnstring = instance.Substring(0, length);

            return returnstring;
        }
    }
}
