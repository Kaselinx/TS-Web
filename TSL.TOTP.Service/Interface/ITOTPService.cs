
using TSL.Base.Platform.Utilities;
using TSL.Common.Model.Service.TOTP;

namespace TSL.TOTP.Service.Interface
{
    public interface ITOTPService
    {
        /// <summary>
        /// Generates a QR code for the provided secret data.
        /// </summary>
        /// <param name="secretData">secret data object</param>
        /// <returns></returns>
        Task<ServiceResult<string>> GenerateQRCodeAsync(string employeeId, string TOTPLabel);


        /// <summary>
        /// Validate totp token with secretkey
        /// </summary>
        /// <param name="totp">totp token</param>
        /// <param name="secretKey">secret key</param>
        /// <returns></returns>
        ServiceResult<string> ValidateTotp( string totp, byte[] secretKey);


        /// <summary>
        /// validate user by employee id and totp token
        /// </summary>
        /// <param name="employeeId">taishin life employee EmpNo</param>
        /// <param name="totp">totp token</param>
        /// <returns></returns>
        Task<ServiceResult<string>> ValidateUserByTotpTokenAsync(string employeeId, string totp);

        /// <summary>
        /// set user active status
        /// </summary>
        /// <param name="employeeId">taishin life employee Id</param>
        /// <param name="isActive">0 fase 1 true</param>
        /// <returns></returns>
        ServiceResult<bool> SetUserActiveStatusByEmployeeId(string employeeId, bool isActive);

        /// <summary>
        /// query existing OTPT user by criteria
        /// </summary>  
        /// <param name="employeeId">employee id</param>
        /// <param name="label">label</param>
        /// <param name="isActive">is currently active</param>
        /// <returns></returns>
        Task<ServiceResult<IEnumerable<SecretDataServiceModel>>> GetOTPTUsersByCriteria(string employeeId, string label, bool isActive);
    }
}
