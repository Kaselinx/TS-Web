
using TSL.Base.Platform.Utilities;
using TSL.Common.Model.DataAccessLayer.OTP;
using TSL.Common.Model.Service.OTP;

namespace TSL.TAAA.Service.Interface
{
    public interface IOTPAuthService
    {
        /// <summary>
        /// if the OTP_AUTH data exists update the data, if not insert the data
        /// </summary>
        /// <param name="otpAuth"></param>
        /// <returns></returns>
        Task<ServiceResult<int>> UpdateInsertOTPData(OTP_AUTHServiceModel otpAuth);

        /// <summary>
        /// check if OTP
        /// </summary>
        /// <param name="seqNo"></param>
        /// <param name="systemID"></param>
        /// <param name="oTP"></param>
        /// <returns></returns>
        Task<ServiceResult<bool>> CheckOTPData(int seqNo, string systemID, string oTP);

        /// <summary>
        /// check if OTP
        /// </summary>
        /// <param name="systemID">system id</param>
        /// <param name="tel_No">tel no</param>
        /// <param name="mail">mail</param>
        /// <param name="oTP">otp code</param>
        /// <param name="effect_Second">effect second</param>
        /// <param name="ipAddress">ip address</param>
        /// <returns></returns>
        Task<ServiceResult<int>> insertOTPData(string systemID, string tel_No, string mail, string oTP, int effect_Second, string ipAddress);


        /// <summary>
        /// Validate OTP token
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="sToken"></param>
        /// <returns></returns>
        Task<bool> TokenValidation(string userId, string sToken);


        /// <summary>
        /// check user ldap status
        /// </summary>
        /// <param name="userId">user id</param>
        /// <param name="password">user...</param>
        /// <returns></returns>
        string CheckUserAdStatus(string userId, string password);

        /// <summary>
        /// mask OTP response
        /// </summary>
        /// <param name="Req"></param>
        /// <returns></returns>
        OTP_Request MaskFunction(OTP_Request Req);


        /// <summary>
        /// mask phone number
        /// </summary>
        /// <param name="Phone"></param>
        /// <returns></returns>
        string MaskPhone(string Phone);

        string MaskMail(string Mail);

        public string UnlockAdAccount(string lockedUserId, string Action);
        string Generate_OTP(OTP_Request maskOTP_Request, string? sessionId, string ipAddress);
    }
}
