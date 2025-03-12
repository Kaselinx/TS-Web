
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using TSL.Common.Model.DataAccessLayer.OTP;

namespace TSL.TAAA.Service.Interface
{
    public interface IOPTMainService
    {
        string GetOTP(OTP_Request OTP_Request);

        string AuthOTP(AuthOTP_Request authOTP_Request);

        string UnlockAdAccount(string lockedUserId, string Action);

        bool SendMail(string eMailAddress, string OTP, string systemID, string functionID, string session, string ipAddress);

        bool SendSMS(string cellphone, string OTP, string systemID, string functionID, string session, string ipAddress);

        bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors);
    }
}
