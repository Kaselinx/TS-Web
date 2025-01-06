
using TSL.Base.Platform.Utilities;
using TSL.Common.Model.Service.OTP;

namespace TSL.TAAA.Service.Interface
{
    public interface IOTP_AuthService
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
    }
}
