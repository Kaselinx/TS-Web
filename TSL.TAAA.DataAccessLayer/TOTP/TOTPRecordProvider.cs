
using Dapper;
using System.Security.Cryptography;
using TSL.Base.Platform.DataAccess;
using TSL.Base.Platform.lnversionOfControl;
using TSL.Base.Platform.Log;
using TSL.Common.Interface.DataAccessLayer.TOTP;
using TSL.Common.Model.DataAccessLayer.TOTP;

namespace TSL.TAAA.DataAccessLayer.TOTP
{

    /// <summary>
    /// TOTP Provider
    /// </summary>
    [RegisterIOC]
    public class TOTPRecordProvider : ITOTPRecordProvider
    {
        private readonly DataAccessService <TAAADataAccessOption> _TAAADBAccessService;
        private readonly ILog<TOTPRecordProvider>? _logger;


        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="tAAADBAcessService">TAAA db</param>
        /// <param name="logger">Log db</param>
        public TOTPRecordProvider(DataAccessService<TAAADataAccessOption> tAAADBAcessService, ILog<TOTPRecordProvider>? logger)
        {
            _TAAADBAccessService = tAAADBAcessService ?? throw new ArgumentNullException(nameof(tAAADBAcessService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger)); ;
        }


        /// <summary>
        /// Insert to TOTP secret data table. 
        /// </summary>
        /// <param name="secretData"></param>
        /// <returns></returns>
        public async Task<SecretData> GetActiveSecretDataByEmployeeIdAsync(string employeeId)
        {
            string  sql = @"SELECT TOP 1 [SecretId]
                              ,[EmployeeId]
                              ,[Label]
                              ,[Secret]
                              ,[IsActive]
                              ,[Create_time]
                              ,[Updated_Time]
                          FROM [TAAA].[dbo].[SecretData]
                          WHERE EmployeeId = @EmployeeId
                            AND IsActive=1"; //only return active secret
            DynamicParameters param = new DynamicParameters();
            param.Add("EmployeeId", employeeId);
            return await _TAAADBAccessService.QueryFirstOrDefaultAsync<SecretData>(sql, param).ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="employeeId"></param>
        /// <param name="isActive"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> UpdateTotpActiveStatus(string employeeId, bool isActive)
        {
                string sql = @"UPDATE [TAAA].[dbo].[SecretData]
                          SET IsActive = @IsActive
                          WHERE EmployeeId = @EmployeeId";
                DynamicParameters param = new DynamicParameters();
                param.Add("EmployeeId", employeeId);
                param.Add("IsActive", isActive);

                int rowsAffected = await _TAAADBAccessService.ExecuteNonQueryAsync(sql, param).ConfigureAwait(false);
                _logger?.Info($"Updated IsActive status for EmployeeId: {employeeId} to {isActive}");
                return rowsAffected > 0; // return true if rows affected
           
        }

        /// <summary>
        /// diable curent user's all OTPT profile and create new one. 
        /// </summary>
        /// <param name="employeeId">taishinlife employee No</param>
        public async Task<int> InsertSecretAsync(SecretData secretData)
        { 
            string sql = @"
                UPDATE [TAAA].[dbo].[SecretData]
                SET IsActive = 0
                WHERE EmployeeId = @EmployeeId;

                INSERT INTO [dbo].[SecretData] ([EmployeeId], [Label], [Secret], [IsActive])
                VALUES (@EmployeeId, @Label, @Secret, 1);
            ";

            var param = new DynamicParameters(new
            {
                secretData.EmployeeId,
                secretData.Label,
                secretData.Secret
            });

            // ExecuteNonQueryAsync will return the total number of affected rows
            int rowsAffected = await _TAAADBAccessService.ExecuteNonQueryAsync(sql, param, true).ConfigureAwait(false);

            _logger?.Info($"Registered OTP user for EmployeeId: {secretData.EmployeeId} with Label: {secretData.Label}");

            return rowsAffected; // This will include the rows affected by both the UPDATE and INSERT queries
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="employeeId"></param>
        /// <param name="label"></param>
        /// <param name="isActive"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<IEnumerable<SecretData>> QueryOPTOUsersByCriteria(string employeeId, string label, bool? isActive)
        {
            string sql = @"SELECT [SecretId]
                              ,[EmployeeId]
                              ,[Label]
                              ,[Secret]
                              ,[IsActive]
                              ,[Create_time]
                              ,[Updated_Time]
                          FROM [TAAA].[dbo].[SecretData]
                          WHERE EmployeeId = @employeeId
                            AND label = @label";

            if (isActive != null)
            {
                sql += " AND IsActive = @isActive"; //only return active secret
            }


            var param = new DynamicParameters(new
            {
                employeeId,
                label,
                isActive
            });

            return await _TAAADBAccessService.QueryAsync<SecretData>(sql, param).ConfigureAwait(false);
        }
    }
}
