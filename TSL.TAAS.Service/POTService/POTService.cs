
using Microsoft.AspNetCore.Http;
using System.Drawing.Text;
using System.Security;
using System.Xml;
using TSL.TAAA.Service.Interface;
using TSL.TAAA.Service.TZLogService;

namespace TSL.TAAA.Service.POTService
{
    public class POTService : IPOTService
    {
        
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOTPAuthService _authService;
        private readonly ITZLogService _tzLogService;

        /// <summary>
        /// access http context
        /// </summary>
        /// <param name="httpContextAccessor">http content accessor</param>
        public POTService(IHttpContextAccessor httpContextAccessor, IOTPAuthService authService)
        {
            _httpContextAccessor = httpContextAccessor;
            _authService = authService;
        }


        /// <summary>
        /// log in to Ad 
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="Password"></param>
        /// <param name="OTPPasswd"></param>
        /// <returns></returns>

        public XmlNode? Login(string UserId, string Password, string OTPPasswd)
        {
            String Status = string.Empty;
            String Role = string.Empty; 
            String ErrorDescription = string.Empty; 
            String User_ID = SecurityElement.Escape(UserId);
            String User_CRD = SecurityElement.Escape(Password);
            String User_OTP = SecurityElement.Escape(OTPPasswd);
            
            //getting user ip address
            String userIP = _httpContextAccessor?.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "";
            String statusMsg = string.Empty;
            try
            {
                bool isPass = _authService.TokenValidation(UserId, OTPPasswd).Result;

                //數位代碼驗證
                if (isPass)
                {
                    statusMsg = "請確認你輸入的LP業務代碼或密碼、數位代碼是否正確！";
                    Status = "21";// 21: 表示數位代碼驗證失敗
                    Role = "";
                    ErrorDescription = statusMsg;
                }

                //AD驗證
                if (statusMsg.Equals(string.Empty))//--啟用 LDAP 驗證處理
                {
                    statusMsg = "?";
                    System.Diagnostics.Trace.Write(new object[] { User_ID, "【登入系統】LDAP開始驗證，帳號[" + User_ID + "]，系統來源:", userIP });
                    //statusMsg = ldapchk.CheckUser(User_ID, User_CRD);
                    statusMsg = _authService.CheckUserAdStatus(User_ID, User_CRD);

                    System.Diagnostics.Trace.Write(new object[] { User_ID, "【登入系統】LDAP驗證結束，帳號[" + User_ID + "]，系統來源:", userIP });
                    if (statusMsg != "") //--LDAP 驗證確認後 處理 LPCode 確認
                    {
                        Status = "22";//22: 表示密碼驗證失敗
                        Role = "";
                        ErrorDescription = statusMsg;
                    }
                }


                if (statusMsg == "") //--LDAP 驗證確認後 處理 LPCode 確認
                {
                    Status = "1";//1: 表示成功
                    //Role = (String)Session["Role"];
                    ErrorDescription = "驗證成功";
                }

            }
            catch (Exception ex)
            {
                statusMsg = "連線驗證Server TimeOut! (" + ex.Message.ToString() + ")";
                Status = "4";
                Role = "";
                ErrorDescription = statusMsg;

            }

            // added by ron 2024/09/16
            _tzLogService.InsertTZLog(User_ID, "", Status, userIP, "TEBS", "LOGIN", 1, "POTService", "Login", "USER", User_ID, "", "", "", "", "");
            //WriteLog writeLog = new WriteLog();
            //writeLog.GetWriteLog("TEBS", User_ID, Role, "Login", Status, ErrorDescription, userIP);
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement resultElement = xmlDoc.CreateElement("Result");
            XmlElement statusElement = xmlDoc.CreateElement("Status");

            string role = string.Empty;
            string status = string.Empty;
            string errorDescription = string.Empty;

            statusElement.InnerText = status;
            resultElement.AppendChild(statusElement);

            var roleElement = xmlDoc.CreateElement("Role");
            roleElement.InnerText = string.IsNullOrEmpty(role) ? "<![CDATA[ ]]>" : role;
            resultElement.AppendChild(roleElement);

            var errorDescriptionElement = xmlDoc.CreateElement("ErrorDescription");
            errorDescriptionElement.InnerText = errorDescription;
            resultElement.AppendChild(errorDescriptionElement);

            xmlDoc.AppendChild(resultElement);
            return xmlDoc?.DocumentElement;
        }
    }
}
