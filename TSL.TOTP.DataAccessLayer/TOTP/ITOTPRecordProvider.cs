
using TSL.TOTP.Model.DataAccessLayer.TOTP;

namespace TSL.TAAA.DataAccessLayer.TOTP
{
    /// <summary>
    ///  TOTP record provider interface
    /// </summary>
    public interface ITOTPRecordProvider
    {
        /// <summary>
        /// Insert to TOTP secret data table. 
        /// </summary>
        /// <param name="secretData"></param>
        /// <returns></returns>
        Task<int> InsertSecretAsync(SecretData secretData);


        /// <summary>
        /// Get current Active secret by employee id
        /// </summary>
        /// <param name="employeeId">taishinlife employee No</param>
        /// <returns></returns>
        Task<IEnumerable<SecretData>> GetActiveSecretDataByEmployeeIdAsync(string employeeId);
    }
}
