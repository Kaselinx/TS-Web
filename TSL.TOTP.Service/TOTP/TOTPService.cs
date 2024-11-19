
using TSL.Base.Platform.Utilities;
using TSL.TOTP.Service.Interface;
using OtpNet;
using QRCoder;
using TSL.Base.Platform.Log;
using TSL.Common.Interface.DataAccessLayer.TOTP;
using TSL.Common.Model.Service.TOTP;
using TSL.Common.Model.DataAccessLayer.TOTP;
using System.Data;
using TSL.Base.Platform.lnversionOfControl;

namespace TSL.TOTP.Service.TOTP
{
    [RegisterIOC]
    public class TOTPService : ITOTPService
    {

        // included services
        private readonly ILog<TOTPService> _logger;
        private ITOTPRecordProvider _iTotpRecordProvider;


        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="log">nlogging provider</param>
        /// <param name="iTotpRecordProvider">iTotp Data Provider</param>
        public TOTPService (ILog<TOTPService> log , ITOTPRecordProvider iTotpRecordProvider)
        {
            _logger = log;
            _iTotpRecordProvider = iTotpRecordProvider;
            _iTotpRecordProvider = iTotpRecordProvider;
        }

        /// <summary>
        /// Generates a QR code for the provided secret data.
        /// </summary>
        /// <param name="secretData">secret data object</param>
        /// <returns></returns>
        public async Task<ServiceResult<string>> GenerateQRCodeAsync(string employeeId, string TOTPLabel)
        {
            _logger.Info(nameof(GenerateQRCodeAsync));
            QRCodeGenerator qRCodeGenerator = new QRCodeGenerator();
            SecretDataServiceModel secretData = new SecretDataServiceModel
            {
                EmployeeId = employeeId, // empid
                Label = TOTPLabel,           // label from system
                Secret = GenerateSecretKey() is byte[] key ? Base32Encoding.ToString(key) : string.Empty 
            };

            var createUrl = GenerateQRCodeUrl(secretData);

            if (createUrl.IsOk)
            {
                QRCodeData data = qRCodeGenerator.CreateQrCode(createUrl.Message, QRCodeGenerator.ECCLevel.Q);
                BitmapByteQRCode qRCode = new BitmapByteQRCode(data);
                byte[] qrCodeBytes = qRCode.GetGraphic(20);
                int result = await _iTotpRecordProvider.InsertSecretAsync(ServiceModelToProviderModel(secretData));
                string base64String = Convert.ToBase64String(qrCodeBytes);
                return new ServiceResult<string>(true, "OK" , base64String);
            }
            else
            {
                return new ServiceResult<string>(false, string.Empty);
            }
        }


        /// <summary>
        /// query OTPT user by criteria
        /// </summary>
        /// <param name="employeeId">employee id</param>
        /// <param name="label">label</param>
        /// <param name="isActive">is currently active</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ServiceResult<IEnumerable<SecretDataServiceModel>>> GetOTPTUsersByCriteria(string employeeId, string label, bool? isActive)
        {
            _logger.Info(nameof(GetOTPTUsersByCriteria), new { employeeId, label, isActive });
            try
            {
                IEnumerable<SecretData> result = await  _iTotpRecordProvider.QueryOPTOUsersByCriteria(employeeId, label, isActive);

                if(result.Any())
                {
                    return new ServiceResult<IEnumerable<SecretDataServiceModel>>(true, "OK", result.Select(x => new SecretDataServiceModel
                    {
                        SecretId = x.SecretId,
                        Secret = x.Secret,
                        Label = x.Label,
                        EmployeeId = x.EmployeeId,
                        Updated_Time = x.Updated_Time,
                        Create_Time = x.Create_Time,
                        isActive = x.isActive
                    }));
                }
                else
                {
                    _logger.Info(nameof(GetOTPTUsersByCriteria), Enumerable.Empty<SecretDataServiceModel>()); 
                    /// return empty list. data not found doesn't mean error.
                    return new ServiceResult<IEnumerable<SecretDataServiceModel>>(true, "OK", Enumerable.Empty<SecretDataServiceModel>());
                }

            }
            catch (Exception ex)
            {
                //log the error and return error message
                _logger.Error(nameof(GetOTPTUsersByCriteria), ex, new { employeeId, label, isActive });
                return new ServiceResult<IEnumerable<SecretDataServiceModel>>(false, ex.Message);
            }
        }


        /// <summary>
        /// update user's active status
        /// </summary>
        /// <param name="employeeId"></param>
        /// <param name="isActive"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ServiceResult<bool>> SetUserActiveStatusByEmployeeId(string employeeId, bool isActive)
        {
            _logger.Info(nameof(SetUserActiveStatusByEmployeeId), new { employeeId, isActive });
            try
            {
                bool result = await _iTotpRecordProvider.UpdateTotpActiveStatus(employeeId, isActive);

                if (result.Equals(true))
                {
                    return new ServiceResult<bool>(true, "User active status updated successfully");
                }
                else
                {
                    return new ServiceResult<bool>(false, "User active status update failed");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(nameof(SetUserActiveStatusByEmployeeId), ex);
                return new ServiceResult<bool>(false, ex.Message);
            }

        }


        /// <summary>
        /// validates the provided TOTP (Time-based One-Time Password) token.
        /// </summary>
        /// <param name="totp">totp token</param>
        /// <param name="secretKey">secret key</param>
        /// <returns></returns>
        public ServiceResult<string> ValidateTotp(string totp, byte[] secretKey)
        {
            _logger.Info(nameof(ValidateTotp), new { totp, secretKey });
            Totp totpGenerator = new Totp(secretKey);
            if (totpGenerator.VerifyTotp(totp, out long timeStepMatched, VerificationWindow.RfcSpecifiedNetworkDelay))
            {
                return new ServiceResult<string>(true, "Valid TOTP token");
            }
            else
            {
                return new ServiceResult<string>(false, "Invalid TOTP token");
            }
        }

        /// <summary>
        /// check if employeeId is active then check if totp is vaild.
        /// </summary>
        /// <param name="employeeId">taishin life employeeNo</param>
        /// <param name="totp">totp token</param>
        /// <returns></returns>
        public async Task<ServiceResult<string>> ValidateUserByTotpTokenAsync(string employeeId, string totp)
        {
            _logger.Info(nameof(ValidateUserByTotpTokenAsync), new { employeeId, totp });
            
            try
            {
                // check if employeeId is active
                SecretData secretData = await _iTotpRecordProvider.GetActiveSecretDataByEmployeeIdAsync(employeeId);
                if (secretData is not null)
                {
                    // then check if totp is valid
                    byte[] secretKey = Base32Encoding.ToBytes(secretData.Secret);
                    return ValidateTotp(totp, secretKey);
                }
                else
                {
                    return new ServiceResult<string>(false, "User Secret data not found");
                }
            }
            catch  (Exception ex)
            {
                _logger.Error(nameof(ValidateUserByTotpTokenAsync), ex);
                return new ServiceResult<string>(false, ex.Message);
            }

        }

        #region private method 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="secretData"></param>
        /// <returns></returns>
        private ServiceResult<string> GenerateQRCodeUrl(SecretDataServiceModel secretData)
        {
            _logger.Info(nameof(GenerateQRCodeUrl), new { secretData });
            try
            {
                string qrCodeUrl = string.Empty;
                qrCodeUrl = $"otpauth://totp/{secretData.Label}?secret={secretData.Secret}&issuer={secretData.EmployeeId}";
                return new ServiceResult<string>(true, qrCodeUrl);
            }
            catch (Exception ex)
            {
                return new ServiceResult<string>(false, ex.Message);
            }
        }



        /// <summary>
        /// validates the provided TOTP (Time-based One-Time Password) token.
        /// </summary>
        /// <param name="totp">totp token</param>
        /// <returns></returns>
        private byte[]? GenerateSecretKey()
        {
            try
            {
                byte[] secretKey = KeyGeneration.GenerateRandomKey();
                return secretKey;
            }
            catch (Exception ex)
            {
                _logger.Error(nameof(GenerateSecretKey), ex);
                return null;
            }
        }

        /// <summary>
        /// Convert ServiceModel to ProviderModel
        /// </summary>
        /// <param name="secretData">secret data</param>
        /// <returns></returns>
        private SecretData ServiceModelToProviderModel(SecretDataServiceModel secretData)
        {
            SecretData providerModel = new SecretData
            {
                SecretId = secretData.SecretId,
                Secret = secretData.Secret,
                Label = secretData.Label,
                EmployeeId = secretData.EmployeeId
            };

            return providerModel;
        }

        ServiceResult<bool> ITOTPService.SetUserActiveStatusByEmployeeId(string employeeId, bool isActive)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
