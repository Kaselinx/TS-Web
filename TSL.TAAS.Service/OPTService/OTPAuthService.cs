
using TSL.Base.Platform.Utilities;
using TSL.Common.Model.Service.OTP;
using TSL.TAAA.Service.Interface;
using TSL.TAAA.DataAccessLayer.OTP;
using TSL.Base.Platform.Log;
using TSL.Base.Platform.lnversionOfControl;
using TSL.Common.Model.DataAccessLayer.OTP;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;
using GSF.Communication.Radius;
using System.Drawing;
using TSL.Base.Platform.Services;
using Microsoft.Extensions.DependencyInjection;
using Org.BouncyCastle.Ocsp;
using GSF.Net.Smtp;
using System.Security.Cryptography;
using Bogus.DataSets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.DataProtection;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace TSL.TAAA.Service.OPTService
{
    [RegisterIOC(IocType.Singleton)]
    public class OTPAuthService : IOTPAuthService
    {
        private readonly IOTP_AuthDataProvider _otp_AuthDataProvider;
        private readonly ILog<OTPAuthService> _logger;
        private readonly RadiusAuthorizationOptions _radiusOptions;
        private readonly IAuthService _authService;
        private readonly IHttpContextAccessor _httpContextAccessor;


        /// <summary>
        /// constroctor
        /// </summary>
        /// <param name="otp_AuthDataProvider">otp auth data table provider</param>
        /// <param name="logger">logger</param>
        /// <param name="radiusOptions">radius options</param>      
        /// <param name="authService">radius options</param>     
        public OTPAuthService(IOTP_AuthDataProvider otp_AuthDataProvider, ILog<OTPAuthService> logger,
            RadiusAuthorizationOptions radiusOptions, IAuthService authService, IHttpContextAccessor httpContextAccessor)
        {
            _otp_AuthDataProvider = otp_AuthDataProvider;
            _logger = logger;
            _radiusOptions = radiusOptions;
            _authService = authService;
            _httpContextAccessor = httpContextAccessor;
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
            int result = 0;
            try
            {
                //check if data existing in table, get all the data by search critia , status =A as active data
                IEnumerable<OTP_AUTH> getAuth = await _otp_AuthDataProvider.GetAuthDataBySearchCritia(otpAuth.SystemID, otpAuth.Tel_No, otpAuth.Mail
                    , otpAuth.CreateTime.Value, otpAuth.EndTime.Value, "A");

                // if result contains any data.
                if (getAuth.Any())
                {
                    if (getAuth.Count() == 1)
                    {
                        //update all current existing data in the table to complete status
                        _ = await _otp_AuthDataProvider.UpdateAuthDataBySeqNo(getAuth.First().SeqNo);
                        // insert a new data 
                        otpAuth.Status = "A";
                       
                    }
                    else
                    {
                        //update all current existing data in the table to complete status
                        _ = await _otp_AuthDataProvider.UpdateAuthDataAllByCritia(otpAuth.SystemID, otpAuth.Tel_No, otpAuth.Mail
                        , otpAuth.CreateTime.Value, otpAuth.EndTime.Value, "C");
                        // insert a new data 
                        otpAuth.Status = "A";
                    }
                    result = await _otp_AuthDataProvider.InsertAuthDatAsync(ServiceModelToProviderModel(otpAuth));
                    return new ServiceResult<int>(true, "OK", result);
                }
                else // if no data then insert the data
                {
                    result = await _otp_AuthDataProvider.InsertAuthDatAsync(ServiceModelToProviderModel(otpAuth));
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


        /// <summary>
        /// Validation radius token
        /// </summary>
        /// <param name="userId">user id</param>
        /// <param name="sToken">radius token</param>
        /// <returns></returns>

        public async Task<bool> TokenValidation( string userId, string sToken)
        {
            // entry logging
            _logger.Info(nameof(TokenValidation), new { userId, sToken } );

            // Try validating with the primary RADIUS server
            bool isValid = await ValidateTokenWithRadiusServer(
                _radiusOptions.RadiusServerPrimary,
                _radiusOptions.RadiusSecretPrimary,
                _radiusOptions.RadiusServerPortPrimary,
                userId,
                sToken
            );

            if (!isValid)
            {
                _logger.Info(nameof(TokenValidation) + "; Secondary Server ", new { userId, sToken });

                // If validation with the primary server fails, try the secondary server
                isValid = await ValidateTokenWithRadiusServer(
                    _radiusOptions.RadiusServerSecondary,
                    _radiusOptions.RadiusSecretSecondary,
                    _radiusOptions.RadiusServerPortSecondary,
                    userId,
                    sToken
                );
            }

            return isValid;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId">user id</param>
        /// <param name="password">ldap password</param>
        /// <returns></returns>
        public string CheckUserAdStatus(string userId, string password)
        {
            // entry logging, params are not loged because password are sensitive data.
            _logger.Info(nameof(CheckUserAdStatus), new { userId });

            try
            {
                bool result = _authService.ADValidateCredentials(userId, password);

                if (result)
                {
                    return string.Empty;
                }
                else
                {
                    return "【登入失敗】請確認您的帳號/密碼是否正確或逾期！(LDAP 驗證錯誤)";
                }
            }
            catch (Exception ex)
            {
                // error logging
                _logger.Error("error", ex, nameof(CheckUserAdStatus));
                return "【登入失敗】(LDAP 驗證錯誤)";
            }
        }



        /// <summary>
        ///  產生6碼OTP
        /// </summary>
        /// <param name="OTP_Request"></param>
        /// <param name="sessionID"></param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public string Generate_OTP(OTP_Request OTP_Request, string sessionID, string ipAddress)
        {
            Random rnd = new Random();
            var otp = string.Empty;
            // Use RandomNumberGenerator static methods instead of RNGCryptoServiceProvider
            byte[] data = new byte[4];
            RandomNumberGenerator.Fill(data);
            // Convert to int.
            Int32 value = BitConverter.ToInt32(data, 0);
            if (value < 0) value = -value;
            otp = value.ToString().Substring(0, 6);

            string v = "OTP No:" + otp;
            _logger.AddHttpLog("Generate_OTP", ipAddress, OTP_Request.SystemID, sessionID, v);
            return otp;
        }

        #region masked methods

        /// <summary>
        /// mask OTP response
        /// </summary>
        /// <param na me="Req"></param>
        public OTP_Request MaskFunction(OTP_Request Req)
        {
            // entry logging
            _logger.Info(nameof(MaskFunction), Req);
            // mask the OTP
            OTP_Request rep = new OTP_Request
            {
                SystemID = Req.SystemID,
                Tel_No = MaskPhone(Req.Tel_No),
                Mail = MaskMail(Req.Mail),
                FunctionKey = Req.FunctionKey,
                Effect_Second = Req.Effect_Second
            };
            return rep;
        }


        /// <summary>
        /// mask phone number
        /// </summary>
        /// <param name="Phone"></param>
        /// <returns></returns>
        public string MaskPhone(string Phone)
        {
            // entry logging
            _logger.Info(nameof(MaskPhone), Phone);

            // Check if the phone number is not null or empty and has at least 7 characters
            if (!string.IsNullOrEmpty(Phone) && Phone.Length >= 7)
            {
                // Mask the phone number and return the value
                return Phone.Replace(Phone.Substring(4, 3), "***");
            }
            else
            {
                return Phone;
            }
        }


        /// <summary>
        ///  mask mail by replacing the first part of the email with asterisks
        /// </summary>
        /// <param name="Mail"></param>
        /// <returns></returns>
        public string MaskMail(string Mail)
        {
            // entry logging
            _logger.Info(nameof(MaskMail), Mail);
            // Check if the email is not null or empty
            if (!string.IsNullOrEmpty(Mail))
            {
                // Mask the email and return the value
                return Mail.Replace(Mail.Substring(0, Mail.IndexOf('@')), "***");
            }
            else
            {
                return Mail;
            }
        }

        /// <summary>
        /// Unlock Active directory account
        /// </summary>
        /// <param name="lockedUserId">user acount need to unlock</param>
        /// <param name="Action">0 unlocked only, 1 unlock and reset the password<</param>
        /// <returns></returns>
        public string UnlockAdAccount(string lockedUserId, string Action)
        {
            // entry logging    
            _logger.Info(nameof(UnlockAdAccount), new { lockedUserId, Action });
            // get the user principal
            using (var context = new PrincipalContext(ContextType.Domain))
            {
                // get the user by user id
                using (var user = UserPrincipal.FindByIdentity(context, lockedUserId))
                {
                    // if user found
                    if (user != null)
                    {
                        // if action is 1 then reset the password
                        if (Action == "1")
                        {
                            // reset the password
                            user.SetPassword(lockedUserId);
                        }
                        // unlock the account
                        user.UnlockAccount();
                        return "OK";
                    }
                    else
                    {
                        return "No user found";
                    }
                }
            }
        }

        #endregion masked methods


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



        /// <summary>
        /// Validate the token with the RADIUS server
        /// </summary>
        /// <param name="server">radius server</param>
        /// <param name="secret">secret password</param>
        /// <param name="ports">ports </param>
        /// <param name="userId">user id </param>
        /// <param name="sToken">token</param>
        /// <returns></returns>
        private async Task<bool> ValidateTokenWithRadiusServer(string server, string secret, int[] ports, string userId, string sToken)
        {
            // entry logging
            _logger.Info(nameof(ValidateTokenWithRadiusServer), new { server, secret, ports, userId, sToken });

            foreach (var port in ports)
            {
                try
                {
                    // Create a new RadiusClient instance
                    using (var radiusClient = new RadiusClient(server, port, secret))
                    {
                        // Create a new RadiusPacket for Access-Request
                        var request = new RadiusPacket
                        {
                            Type = PacketType.AccessRequest,
                            Identifier = (byte)new Random().Next(0, 256),
                            Authenticator = RadiusPacket.CreateRequestAuthenticator(secret)
                        };

                        // Add attributes to the request
                        request.Attributes.Add(new RadiusPacketAttribute(AttributeType.UserName, RadiusPacket.Encoding.GetBytes(userId)));
                        request.Attributes.Add(new RadiusPacketAttribute(AttributeType.UserPassword, RadiusPacket.EncryptPassword(sToken, secret, request.Authenticator)));

                        // Send the request and get the response
                        var response = await Task.Run(() => radiusClient.ProcessRequest(request));

                        // Check if the response is an Access-Accept
                        if (response.Type == PacketType.AccessAccept)
                        {
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error validating token with RADIUS server {server} on port {port}", ex, new { server, secret, ports, userId, sToken });
                }
            }

            return false;
        }



        /// <summary>
        /// insert data to OTP_AUTH
        /// </summary>
        /// <param name="systemID"></param>
        /// <param name="tel_No"></param>
        /// <param name="mail"></param>
        /// <param name="oTP"></param>
        /// <param name="effect_Second"></param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ServiceResult<int>> insertOTPData(string systemID, string tel_No, string mail, string oTP, int effect_Second, string ipAddress)
        {
            // entry logging
            _logger.Info(nameof(insertOTPData), new { systemID, tel_No, mail, oTP, effect_Second, ipAddress});
            try
            {
                OTP_AUTHServiceModel oTP_AUTH = new OTP_AUTHServiceModel
                {
                    SystemID = systemID,
                    Tel_No = tel_No,
                    Mail = mail,
                    OTP = oTP,
                    Status = "A", // status as active for active data
                    Effect_Second = effect_Second,
                    SourceIP = ipAddress
                };
                var result = await this.UpdateInsertOTPData(oTP_AUTH);

                if (result.IsOk )
                {
                    return new ServiceResult<int>(true, "OK", result.Data);
                }
                else
                {
                    return new ServiceResult<int>(false, "Failed to insert OTP data", 0);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error on" + nameof(insertOTPData), ex,  new { systemID, tel_No, mail, oTP, effect_Second, ipAddress });
                return new ServiceResult<int>(false, ex.Message, 0);
            }
        }

        #endregion
    }
}
