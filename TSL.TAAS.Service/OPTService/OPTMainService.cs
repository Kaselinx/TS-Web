
using Bogus.DataSets;
using GSF;
using GSF.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using MySqlX.XDevAPI.Common;
using Newtonsoft.Json;
using Org.BouncyCastle.Utilities;
using System.Dynamic;
using System.Net;
using System.Net.Security;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using TSL.Base.Platform.Log;
using TSL.Base.Platform.SystemInfo;
using TSL.Base.Platform.Utilities;
using TSL.Common.Model.DataAccessLayer.OTP;
using TSL.TAAA.Service.Interface;
using TSL.TAAA.Service.MsgWSDLService;

namespace TSL.TAAA.Service.OPTService
{
    public class OPTMainService : IOPTMainService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOTPAuthService _otpAuthService;
        private readonly IMailUtil _mailUtil;
        private readonly IOptions<GeneralOption> _option;
        private readonly IOptions<WordingSetterOption> _wordOption;
        private readonly ILog<OTPAuthService> _logger;
       

        /// <summary>
        /// /
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <param name="otpAuthService"></param>
        /// <param name="option"></param>
        /// <param name="wordOption"></param>
        /// <param name="mailutil"></param>
        public OPTMainService(IHttpContextAccessor httpContextAccessor, IOTPAuthService otpAuthService, IOptions<GeneralOption> option
            , IOptions<WordingSetterOption> wordOption, IMailUtil mailutil, ILog<OTPAuthService> logger)   
        {
            _httpContextAccessor = httpContextAccessor;
            _otpAuthService = otpAuthService;
            _mailUtil = mailutil;
            _option = option;
            _wordOption = wordOption;
            _logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="authOTP_Request">auth request</param>
        /// <returns></returns>
        public string AuthOTP(AuthOTP_Request authOTP_Request)
        {
            string ipAddress = "";
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                string? xAdrress = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                string? remoteAddress = httpContext.Connection.RemoteIpAddress?.ToString();

                if (!String.IsNullOrEmpty(xAdrress))
                {
                    ipAddress = xAdrress.Split(',')[0];
                }
                else if (!String.IsNullOrEmpty(remoteAddress))
                {
                    ipAddress = remoteAddress;
                }
            }

            bool result = true;
            string msg = string.Empty;
            
            //string json = JsonConvert.SerializeObject(authOTP_Request);
            string json = System.Text.Json.JsonSerializer.Serialize(authOTP_Request);

            _logger.Info(nameof(AuthOTP), new { authOTP_Request.SystemID, authOTP_Request.SeqNo, authOTP_Request.OTP, ipAddress });
            //no need TAAA log //OTPServices.InsertTAAALog(AuthOTP_Request.SystemID, "AuthOTP_Request", json, Session.SessionID, ipAddress);

            bool chkResult = StringExtension.check_RequestData(authOTP_Request);
            if (!chkResult)
            {
                return System.Text.Json.JsonSerializer.Serialize(new OTP_Response { Result = false, Msg = "OTP資料不正確" });

            }
            else
            {
                _logger.Info(nameof(AuthOTP), new { authOTP_Request.SystemID, authOTP_Request.SeqNo, authOTP_Request.OTP, ipAddress });
                // OTPServices.InsertTAAALog(AuthOTP_Request.SystemID, "OTP Check Start", AuthOTP_Request.SeqNo + " : " + AuthOTP_Request.OTP, Session.SessionID, ipAddress);
                int seqno = 0;
                bool success = int.TryParse(authOTP_Request.SystemID, out seqno);

                if (!success)
                {
                    return System.Text.Json.JsonSerializer.Serialize(new OTP_Response { Result = false, Msg = "OTP資料不正確" });
                }

                var checkResult = _otpAuthService.CheckOTPData(seqno, authOTP_Request.SeqNo, authOTP_Request.OTP).Result;

                if ( checkResult.IsOk)
                {
                    msg = "OTP驗證成功";
                }
                else
                {
                    result = false;
                    msg = "OTP驗證失敗";
                }

                _logger.Info(nameof(AuthOTP), new { authOTP_Request.SystemID, authOTP_Request.SeqNo, authOTP_Request.OTP, ipAddress, V = checkResult.IsOk ? "驗證成功" : "驗證失敗" });
            }
            return System.Text.Json.JsonSerializer.Serialize(new AuthOTP_Response { Result = result , Msg = msg });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="certificate"></param>
        /// <param name="chain"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }


        public string GetOTP(OTP_Request OTP_Request)
        {
            string ipAddress = "";
            var httpContext = _httpContextAccessor.HttpContext;
            string? sessionId = httpContext?.Session.Id;

            //repace HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            string? xAdrress = httpContext?.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            //repace HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            string? remoteAddress = httpContext?.Connection.RemoteIpAddress?.ToString();

            if (!string.IsNullOrEmpty(xAdrress))
            {
                ipAddress = xAdrress.Split(',')[0];
            }
            else if (!string.IsNullOrEmpty(remoteAddress))
            {
                ipAddress = remoteAddress;
            }

            bool Result = true;
            string Msg = "";

            //OTPAuthService OTPServices = new OTPAuthService();
            var MaskOTP_Request = _otpAuthService.MaskFunction(OTP_Request);//遮罩處理
            string json = JsonConvert.SerializeObject(MaskOTP_Request);
            
            //OTPServices.InsertTAAALog(MaskOTP_Request.SystemID, "OTP_Request", json, Session.SessionID, ipAddress);
            _logger.Info(nameof(GetOTP), new { MaskOTP_Request.SystemID, MaskOTP_Request.Mail, MaskOTP_Request.Tel_No, ipAddress });
            //filter data
            bool chkResult = StringExtension.check_RequestData(OTP_Request);
            if (!chkResult)
            {
                return System.Text.Json.JsonSerializer.Serialize(new OTP_Response { Result = false, Msg = "OTP資料不正確" });
            }
            else
            {
                //input data log  
                if (string.IsNullOrEmpty(MaskOTP_Request.Mail) && string.IsNullOrEmpty(MaskOTP_Request.Tel_No))
                {
                    Result = false;
                    Msg = "未收到電話及eMail資訊";
                }
                else
                {
                    string? OTP = _otpAuthService.Generate_OTP(MaskOTP_Request, sessionId, ipAddress);
                    var insertResult = _otpAuthService.insertOTPData(MaskOTP_Request.SystemID, MaskOTP_Request.Tel_No, MaskOTP_Request.Mail, OTP, MaskOTP_Request.Effect_Second, ipAddress).Result;
                    
                   // OTPServices.InsertTAAALog(MaskOTP_Request.SystemID, "OTP insert result", "OK:" + insertResult, Session.SessionID, ipAddress);
                    _logger.Info(Msg, new { MaskOTP_Request.SystemID, MaskOTP_Request.Mail, MaskOTP_Request.Tel_No, ipAddress, insertResult });
                    if (insertResult.IsOk)
                    {
                        return System.Text.Json.JsonSerializer.Serialize(new OTP_Response { Result = false, Msg = "OTP驗證碼產生失敗" });
                    }
                    else
                    {
                        Msg = insertResult.Message;
                    }

                    if (!string.IsNullOrEmpty(OTP_Request.Mail))
                    {
                        //OTPServices.InsertTAAALog(MaskOTP_Request.SystemID, "SendMail", "有Mail資料，進行發送eMail", Session.SessionID, ipAddress);
                        _logger.Info(nameof(GetOTP), new { MaskOTP_Request.SystemID, v = "SendMail", v2 = "有Mail資料，進行發送eMail", sessionId, ipAddress });  
                        bool sendMailResult = SendMail(OTP_Request.Mail, OTP, OTP_Request.SystemID, OTP_Request.FunctionKey, sessionId, ipAddress);
                        //new OTPAuthService().InsertTAAALog(MaskOTP_Request.SystemID, "OTP_SendMail", sendMailResult == true ? "eMail發送成功，OTP:" + OTP : "eMail發送失敗，OTP:" + OTP, Session.SessionID, ipAddress);
                        _logger.Info(nameof(GetOTP), new { MaskOTP_Request.SystemID, v = "OTP_SendMail", v2 = sendMailResult == true ? "eMail發送成功，OTP:" + OTP : "eMail發送失敗，OTP:" + OTP, sessionId, ipAddress });    
                        Result = sendMailResult;
                        Msg = sendMailResult == true ? insertResult.Message : "eMail發送失敗";
                    }
                    if (!string.IsNullOrEmpty(OTP_Request.Tel_No))
                    {
                        //OTPServices.InsertTAAALog(MaskOTP_Request.SystemID, "SendSMS", "有SMS資料，進行發送SMS", Session.SessionID, ipAddress);
                        var sendSMSResult = SendSMS(OTP_Request.Tel_No, OTP, OTP_Request.SystemID, OTP_Request.FunctionKey, sessionId, ipAddress);
                        //new OTPAuthService().InsertTAAALog(MaskOTP_Request.SystemID, "OTP_SendSMS", sendSMSResult == true ? "SMS發送成功，OTP:" + OTP : "SMS發送失敗，OTP:" + OTP, Session.SessionID, ipAddress);
                        _logger.Info(nameof(GetOTP), new { MaskOTP_Request.SystemID, v = "OTP_SendSMS", v2 = sendSMSResult == true ? "SMS發送成功，OTP:" + OTP : "SMS發送失敗，OTP:" + OTP, sessionId, ipAddress });  
                        Result = sendSMSResult;
                        Msg = sendSMSResult == true ? insertResult.Message : "SMS發送失敗";
                    }
                }

            }
            return System.Text.Json.JsonSerializer.Serialize(new OTP_Response { Result = Result, Msg = Msg });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eMailAddress"></param>
        /// <param name="OTP"></param>
        /// <param name="systemID"></param>
        /// <param name="functionID"></param>
        /// <param name="session"></param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public bool SendMail(string eMailAddress, string OTP, string systemID, string functionID, string session, string ipAddress)
        {
            _logger.Info(nameof(SendMail), new { eMailAddress, OTP, systemID, functionID, session, ipAddress });
            string mailsender = _option.Value.Mailsender;
            string? subject = _wordOption.Value?.PotOTPMail?.Subject;
            string? mailContent = _wordOption.Value?.PotOTPMail?.Content;

            string sender = mailsender;

            StringBuilder mailbody = new StringBuilder();
            if (mailContent != null)
            {
                mailbody.AppendFormat(mailContent, OTP);
            }
            else
            {
                // Handle the case where mailContent is null
                mailbody.Append("Default mail content not found: " + OTP);
            }
            bool result = true;
            try
            {
                //寄送郵件
                _mailUtil.SendMail(sender, eMailAddress, "", subject, mailbody.ToString());
            }
            catch (Exception ex)
            {
                _logger.Error(nameof(SendMail), ex, new { eMailAddress, OTP, systemID, functionID, session, ipAddress });
            }

            return result;
        }

        /// <summary>
        /// send sms message
        /// </summary>
        /// <param name="cellphone"></param>
        /// <param name="OTP"></param>
        /// <param name="systemID"></param>
        /// <param name="functionID"></param>
        /// <param name="session"></param>
        /// <param name="ipAddress"></param>
        /// <returns></returns>
        public bool SendSMS(string cellphone, string OTP, string systemID, string functionID, string session, string ipAddress)
        {
            bool result = true;
            try
            {
                ////送SMS
                //
                string? subject = _wordOption.Value?.PotOTPSms?.Subject;
                string? content = _wordOption.Value?.PotOTPSms?.Content;

                //WordingInfo Wording_info = SysConfig.Instance.GetWordingInfoConfig(systemID, functionID + "_SMS");

                //以下寄送SMS
               // MsgServiceSoapClient ws = new MsgServiceSoapClient();
                SingleMsgObject SendMsgObject = new SingleMsgObject();
                SingleMsgResultObject SendMsgResultObject = new SingleMsgResultObject();
                StringBuilder mailbody = new StringBuilder();
                mailbody.AppendFormat(content, OTP);

                SendMsgObject.Media_Type = "1";         //訊息類別
                SendMsgObject.System_ID = systemID;       //發送系統代號
                SendMsgObject.Subject = subject; //發送主旨
                                                              //Attachment image = new Attachment(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Content\img\PWDProcess.png"));
                SendMsgObject.Message = mailbody.ToString(); //發送訊息內容
                SendMsgObject.Dept_ID = systemID;         //發訊者部門
                SendMsgObject.Employee_ID = "System";   //發訊者
                SendMsgObject.Content_Type = "新增簡訊";//內容分類
                SendMsgObject.User_No = "16";           //字串填入的內容為數字
                SendMsgObject.Priority = "1";           //1優先處理0一般處理
                SendMsgObject.LiveTime = null;          //簡訊存活期間
                SendMsgObject.Target = cellphone;       //寄送目標電話
                SendMsgObject.Deliver_Date = null;      //預約發送時間
                SendMsgObject.POL_ID = "";             //保單號碼
                SendMsgObject.CLI_ID = "";         //客戶代號
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;// SecurityProtocolType.Tls1.2;

                //SendMsgResultObject = SendMsgObject.SetSingleMsgData(SendMsgObject);
            }
            catch (Exception ex)
            {
                //new OTPAuthService().InsertTAAALog(systemID, "SendSMS fail", ex.Message, session, ipAddress);
                _logger.Error(nameof(SendSMS), ex, new { cellphone, OTP, systemID, functionID, session, ipAddress });   
                result = false;
            }

            return result;
        }


        /// <summary>
        /// unlock under account
        /// </summary>
        /// <param name="lockedUserId">account require to be unlock</param>
        /// <param name="Action">change password only or not</param>
        /// <returns></returns>
        public string UnlockAdAccount(string lockedUserId, string Action)
        {
            string msg = _otpAuthService.UnlockAdAccount(lockedUserId, Action);
            string Msg = "";
            bool Result = true;

            // if change failed
            if (msg != "更改密碼成功")
            {
                Result = false;
            }
            else // success
            {
                Result = true;
            }
            Msg = msg;
            return System.Text.Json.JsonSerializer.Serialize(new AuthOTP_Response { Result = Result, Msg = Msg });
        }
    }
}
