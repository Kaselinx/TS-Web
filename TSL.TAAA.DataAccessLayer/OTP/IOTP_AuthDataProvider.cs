


using TSL.Common.Model.DataAccessLayer.OTP;

namespace TSL.TAAA.DataAccessLayer.OTP
{
    public interface IOTP_AuthDataProvider
    {

        /// <summary>
        /// query data by id
        /// </summary>
        /// <param name="seqNo">sql numer</param    >
        /// <returns></returns>
        Task<OTP_AUTH> GetAuthDataBySeqNo(int seqNo);

        /// <summary>
        /// search auth data needs to be update
        /// </summary>
        /// <param name="sytemId">taishin system short name</param>
        /// <param name="telNo">tel no</param>
        /// <param name="mail">mail</param>
        /// <param name="createTime">create Time</param>
        /// <param name="EndTime">end time</param>
        /// <param name="status">status</param>
        /// <returns></returns>
        Task<IEnumerable<OTP_AUTH>> GetAuthDataBySearchCritia(string sytemId, string telNo, string mail, DateTime createTime , DateTime EndTime, string status);


        /// <summary>
        /// upate data by seq no
        /// </summary>
        /// <param name="seqNo">seq no</param>
        /// <returns></returns>
        Task<int> UpdateAuthDataBySeqNo(int seqNo);


        /// <summary>
        ///  omsert data to OTP_AUTH
        /// </summary>
        /// <param name="oTP_AUTH">OTP_AUTH object</param>
        /// <returns></returns>
        Task<int> InsertAuthDatAsync(OTP_AUTH oTP_AUTH);


        /// <summary>
        /// update all auth data by critia
        /// </summary>
        /// <param name="sytemId">taishin system short name</param>
        /// <param name="telNo">tel no</param>
        /// <param name="mail">mail</param>
        /// <param name="createTime">create Time</param>
        /// <param name="EndTime">end time</param>
        /// <param name="status">status</param>
        /// <returns></returns>
        Task<int> UpdateAuthDataAllByCritia(string sytemId, string telNo, string mail, DateTime createTime, DateTime EndTime, string status);


    }
}
