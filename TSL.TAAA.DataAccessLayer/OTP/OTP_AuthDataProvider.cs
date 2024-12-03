using System.ComponentModel.DataAnnotations;
using TSL.Base.Platform.DataAccess;
using TSL.Base.Platform.Log;
using TSL.Common.Model.DataAccessLayer.OTP;
using TSL.Common.Model.DataAccessLayer.TSTD;
using TSL.TAAA.DataAccessLayer.TOTP;

namespace TSL.TAAA.DataAccessLayer.OTP
{
    public class OTP_AuthDataProvider : IOTP_AuthDataProvider
    {
        private readonly DataAccessService<TAAADataAccessOption> _TAAADBAccessService;
        private readonly ILog<TOTPRecordProvider>? _logger;

        /// <summary>
        ///  constructor
        /// </summary>
        /// <param name="tAAADBAcessService"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public OTP_AuthDataProvider(DataAccessService<TAAADataAccessOption> tAAADBAcessService, ILog<TOTPRecordProvider>? logger)
        {
            _TAAADBAccessService = tAAADBAcessService ?? throw new ArgumentNullException(nameof(tAAADBAcessService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger)); ;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sytemId"></param>
        /// <param name="telNo"></param>
        /// <param name="mail"></param>
        /// <param name="createTime"></param>
        /// <param name="EndTime"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public Task<IEnumerable<OTP_AUTH>> GetAuthDataBySearchCritia(string sytemId, string telNo, string mail, DateTime createTime, DateTime EndTime, string status)
        {
            _logger?.Info("GetAuthDataBySearchCritia", new { sytemId,  telNo,  mail,  createTime,  EndTime,  status });
            string query = @"SELECT TOP 1 [SeqNo]
                              ,[SystemID]
                              ,[Tel_No]
                              ,[Mail]
                              ,[OTP]
                              ,[Status]
                              ,[CreateTime]
                              ,[EndTime]
                              ,[Effect_Second]
                              ,[Status_ModiDate]
                              ,[SourceIP]
                          FROM [TAAA].[dbo].[OTP_AUTH]
                          WHERE SystemID = @SystemID
                            AND (Tel_No = @Tel_No
                            AND Mail = @Mail)
                            AND CreateTime = @CreateTime
                            AND EndTime = @EndTime
                            AND Status = @Status";

            dynamic param = new { SystemID = sytemId, Tel_No = telNo, Mail = mail, CreateTime = createTime, EndTime = EndTime, Status = status };

            return _TAAADBAccessService.QueryAsync<OTP_AUTH>(query, param);
        }

        /// <summary>
        /// query Auth Data by primary key
        /// </summary>
        /// <param name="seqNo">primary key</param>
        /// <returns></returns>
        public Task<OTP_AUTH> GetAuthDataBySeqNo(int seqNo)
        {
            _logger?.Info("GetAuthDataBySeqNo", seqNo);
            return  _TAAADBAccessService.QueryById<OTP_AUTH>(seqNo);
        }

        /// <summary>
        /// insert to OTP_AUTH
        /// </summary>
        /// <param name="oTP_AUTH"></param>
        /// <returns></returns>
        public async Task<int> InsertAuthDatAsync(OTP_AUTH oTP_AUTH)
        {
            _logger?.Info("InsertAuthDatAsync", oTP_AUTH);

            int rowsAffected = await _TAAADBAccessService.InsertAsync(oTP_AUTH, false, false);
            return rowsAffected;
        }


        /// <summary>
        /// update all `OTP_AUTH` data by critia
        /// </summary>
        /// <param name="sytemId"></param>
        /// <param name="telNo"></param>
        /// <param name="mail"></param>
        /// <param name="createTime"></param>
        /// <param name="EndTime"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public Task<int> UpdateAuthDataAllByCritia(string sytemId, string telNo, string mail, DateTime createTime, DateTime EndTime, string status)
        {
            _logger?.Info("UpdateAuthDataAllByCritia", new { sytemId, telNo, mail, createTime, EndTime, status });
            string sql = @"UPDATE [OTP_AUTH]
                            SET [Status]='C', Status_ModiDate=getdate()
                            WHERE SystemID = @SystemID
                            AND (Tel_No = @Tel_No
                            AND Mail = @Mail)
                            AND CreateTime = @CreateTime
                            AND EndTime = @EndTime
                            AND Status = @Status";

            dynamic param = new { SystemID = sytemId, Tel_No = telNo, Mail = mail, CreateTime = createTime, EndTime = EndTime, Status = status };

            return _TAAADBAccessService.ExecuteNonQueryAsync(sql, param);
        }

        /// <summary>
        /// update exist data by seqno...
        /// </summary>
        /// <param name="seqNo"></param>
        /// <returns></returns>
        public async Task<int> UpdateAuthDataBySeqNo(int seqNo)
        {
            string sql = @"UPDATE [dbo].[OTP_AUTH]
                           SET [Status]='C', Status_ModiDate=getdate()
                           WHERE SeqNo = @SeqNo";
            return await _TAAADBAccessService.ExecuteNonQueryAsync(sql, new { SeqNo = seqNo });

        }

    }
}
