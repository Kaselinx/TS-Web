
using TSL.Base.Platform.Utilities;
using TSL.Common.Model.Service.OTP;
using TSL.TAAA.Service.Interface;
using TSL.TAAA.DataAccessLayer.OTP;
using TSL.Base.Platform.Log;
using TSL.Base.Platform.lnversionOfControl;
using System.Collections.Generic;
using TSL.Common.Model.DataAccessLayer.OTP;
using System.Drawing;
using TSL.Common.Model.DataAccessLayer.TSTD;
using TSL.Common.Model.Service.TSTD;

namespace TSL.TAAA.Service.OPT
{
    [RegisterIOC]
    public class OTP_AuthService : IOTP_AuthService
    {
        private readonly IOTP_AuthDataProvider _otp_AuthDataProvider;
        private readonly ILog<OTP_AuthService> _logger;

        /// <summary>
        /// constroctor
        /// </summary>
        /// <param name="otp_AuthDataProvider">otp auth data table provider</param>
        /// <param name="logger">logger</param>
        public OTP_AuthService(IOTP_AuthDataProvider otp_AuthDataProvider, ILog<OTP_AuthService> logger)
        {
            _otp_AuthDataProvider = otp_AuthDataProvider;
            _logger = logger;
        }


        /// <summary>
        ///  check OPT data with Seqno,  systemID and OTP
        /// </summary>
        /// <param name="SeqNo"></param>
        /// <param name="SystemID"></param>
        /// <param name="OTP"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ServiceResult<bool>> CheckOTPData(int seqNo, string systemID, string oTP)
        {
            // entry logging
            _logger.Info(nameof(CheckOTPData), new { seqNo, systemID, oTP });

            try
            {
                // get result by Id
                OTP_AUTH result = await _otp_AuthDataProvider.GetAuthDataBySeqNo(seqNo);

                //if data found
                if (result != null)
                {
                    // data found
                    if (result.SystemID == systemID && result.OTP == oTP)
                    {
                        return new ServiceResult<bool>(true, "OK", true);
                    }
                }

                /// no data found
                return new ServiceResult<bool>(false, "No data found", false);

            }
            catch (Exception ex)
            {
                // error logging
                _logger.Error("error", ex, nameof(CheckOTPData));
                // return false result
                return new ServiceResult<bool>(false, ex.Message, false);
            }
        }

        /// <summary>
        /// if the OTP_AUTH data exists update the data, if not insert the data
        /// </summary>
        /// <param name="otpAuth">otp auth object</param>
        /// <returns></returns>
        public async Task<ServiceResult<int>> UpdateInsertOTPData(OTP_AUTHServiceModel otpAuth)
        {
            // entry logging
            _logger.Info(nameof(UpdateInsertOTPData), otpAuth);
            try
            {
                //check if data existing in table
                IEnumerable<OTP_AUTH> getAuth = await _otp_AuthDataProvider.GetAuthDataBySearchCritia(otpAuth.SystemID, otpAuth.Tel_No, otpAuth.Mail
                    , otpAuth.CreateTime.Value, otpAuth.EndTime.Value, otpAuth.Status);


                // if result contains any data.
                if (getAuth.Any())
                {
                    if (getAuth.Count() == 1)
                    {
                        // update the data if only one record found
                        _ = await _otp_AuthDataProvider.UpdateAuthDataBySeqNo(getAuth.First().SeqNo);
                        return new ServiceResult<int>(true, "OK", getAuth.First().SeqNo);
                    }
                    else
                    {
                        //update all the data fit in critia.
                        _ = await _otp_AuthDataProvider.UpdateAuthDataAllByCritia(otpAuth.SystemID, otpAuth.Tel_No, otpAuth.Mail
                        , otpAuth.CreateTime.Value, otpAuth.EndTime.Value, otpAuth.Status);
                        return new ServiceResult<int>(true, "OK", getAuth.First().SeqNo);
                    }
                }
                else // if no data then insert the data
                {
                    int result = await _otp_AuthDataProvider.InsertAuthDatAsync(ServiceModelToProviderModel(otpAuth));
                    return new ServiceResult<int>(true, "OK", result);
                }
            } 
            catch (Exception ex)
            {
                // error logging
                _logger.Error("error", ex, nameof(UpdateInsertOTPData));
                // return false result
                return new ServiceResult<int>(false, ex.Message);
            }
        }


        #region private methods
        /// <summary>
        /// convert from provider model to service. 
        /// </summary>
        /// <param name="oTP_AUTH">provider model. </param>
        /// <returns></returns>
        private OTP_AUTHServiceModel ProviderToServiceModel(OTP_AUTH oTP_AUTH)
        {
            OTP_AUTHServiceModel ServiceModel = new OTP_AUTHServiceModel
            {
                CreateTime = oTP_AUTH.CreateTime,
                SeqNo = oTP_AUTH.SeqNo,
                SourceIP = oTP_AUTH.SourceIP,
                Status = oTP_AUTH.Status,
                Effect_Second = oTP_AUTH.Effect_Second,
                EndTime = oTP_AUTH.EndTime,
                OTP = oTP_AUTH.OTP,
                SystemID = oTP_AUTH.SystemID,
                Tel_No = oTP_AUTH.Tel_No,
                Mail = oTP_AUTH.Mail,
                Status_ModiDate = oTP_AUTH.Status_ModiDate
            };

            return ServiceModel;
        }

        /// <summary>
        /// convert from provider model to service. 
        /// </summary>
        /// <param name="oTP_AUTHServiceModel">service model. </param>
        /// <returns></returns>
        private OTP_AUTH ServiceModelToProviderModel(OTP_AUTHServiceModel oTP_AUTHServiceModel)
        {
            OTP_AUTH oTP_AUTH = new OTP_AUTH
            {
                CreateTime = oTP_AUTHServiceModel.CreateTime,
                SeqNo = oTP_AUTHServiceModel.SeqNo,
                SourceIP = oTP_AUTHServiceModel.SourceIP,
                Status = oTP_AUTHServiceModel.Status,
                Effect_Second = oTP_AUTHServiceModel.Effect_Second,
                EndTime = oTP_AUTHServiceModel.EndTime,
                OTP = oTP_AUTHServiceModel.OTP,
                SystemID = oTP_AUTHServiceModel.SystemID,
                Tel_No = oTP_AUTHServiceModel.Tel_No,
                Mail = oTP_AUTHServiceModel.Mail,
                Status_ModiDate = oTP_AUTHServiceModel.Status_ModiDate
            };

            return oTP_AUTH;
        }


        #endregion
    }
}
