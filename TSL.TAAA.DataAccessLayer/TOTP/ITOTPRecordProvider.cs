
using TSL.Common.Model.DataAccessLayer.TOTP;

namespace TSL.Common.Interface.DataAccessLayer.TOTP
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
        Task<SecretData> GetActiveSecretDataByEmployeeIdAsync(string employeeId);

        /// <summary>
        /// update employee status
        /// </summary>
        /// <param name="employeeId">employee id</param>
        /// <param name="isActive">0 deactive 1 active</param>
        /// <returns></returns>
        Task<bool> UpdateTotpActiveStatus(string employeeId, bool isActive);


        /// <summary>
        /// Query OTP users by criteria
        /// </summary>
        /// <param name="employeeId">employee id</param>
        /// <param name="label">label</param>
        /// <param name="isActive">is active or not</param>
        /// <returns></returns>
        Task<IEnumerable<SecretData>> QueryOPTOUsersByCriteria(string employeeId, string label, bool? isActive);
    }
}
