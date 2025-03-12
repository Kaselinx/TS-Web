using Microsoft.Extensions.Options;
using System.ComponentModel;
using System.Net.Mail;
using System.Text;
using TSL.Base.Platform.lnversionOfControl;

namespace TSL.Base.Platform.Utilities
{

    [RegisterIOC(IocType.Singleton)]
    public class MailUtil : IMailUtil, IDisposable
    {
        private readonly IOptions<GeneralOption> _option;
        private readonly SmtpClient client;
        public MailUtil(IOptions<GeneralOption> option)
        {
            _option = option;
            client = new SmtpClient(_option.Value.SmtpServer);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromAddress"></param>
        /// <param name="fromName"></param>
        /// <param name="toAddress"></param>
        /// <param name="toName"></param>
        /// <param name="ccAddress"></param>
        /// <param name="ccName"></param>
        /// <param name="mailSubject"></param>
        /// <param name="mailBody"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void SendMail(string fromAddress, string fromName, string toAddress, string toName,
                 string ccAddress, string ccName, string mailSubject, string mailBody)
        {
            var errorMessages = new List<string>();

            MailAddress? fromAdd = CreateMailAddress(fromAddress, fromName, "寄件者信箱格式錯誤", errorMessages);
            MailAddress? toAdd = CreateMailAddress(toAddress, toName, "收件者信箱格式錯誤", errorMessages);
            MailAddress? ccAdd = CreateMailAddress(ccAddress, ccName, "副本收件者信箱格式錯誤", errorMessages);

            if (errorMessages.Count != 0)
            {
                throw new InvalidOperationException(string.Join(Environment.NewLine, errorMessages));

            }

            MailMessage msg = new MailMessage(fromAdd, toAdd)
            {
                Subject = mailSubject,
                Body = mailBody
            };

            if (ccAdd != null)
            {
                msg.CC.Add(ccAdd);
            }

            _ = ToSend(msg);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fromAddress"></param>
        /// <param name="toAddress"></param>
        /// <param name="ccAddress"></param>
        /// <param name="mailSubject"></param>
        /// <param name="mailBody"></param>
        /// <exception cref="ApplicationException"></exception>
        public void SendMail( string fromAddress, string toAddress, string ccAddress,string mailSubject, string mailBody)
        {
            fromAddress = fromAddress.Trim();
            toAddress = toAddress.Trim();
            ccAddress = ccAddress.Trim();

            //MailInfo original_info = SysConfig.Instance.GetMailInfoConfig();
            //this.SmtpClient = new SmtpClient(original_info.smtp);

            StringBuilder errorMailAddress = new StringBuilder();
            StringBuilder errorSendAdrress = new StringBuilder();
            MailAddress fromAdd = null;
            List<MailAddress> toAdd = new List<MailAddress>();
            List<MailAddress> ccAdd = new List<MailAddress>();

            if (!string.IsNullOrEmpty(fromAddress))
            {
                if (!IsValidEmail(fromAddress))
                {
                    _ = errorMailAddress.AppendLine("* 寄件者信箱格式錯誤");
                }
                else
                {
                    fromAdd = new MailAddress(fromAddress);
                }
            }

            // if not empty, split the email address by ';'
            if (!string.IsNullOrEmpty(toAddress))
            {
                List<string> items = toAddress.Split(';').ToList();
                // Remove the item in the list if the email address is not correct.
                _ = items.RemoveAll(o => !IsValidEmail(o));
                // Add the rest of the email addresses to the list.
                toAdd.AddRange(items.Select(o => new MailAddress(o)));
            }

            // if not empty, split the ccemail address by ';'

            if (!string.IsNullOrEmpty(ccAddress))
            {
                List<string> ccitems = ccAddress.Split(';').ToList();
                // Remove the item in the list if the email address is not correct.
                _ = ccitems.RemoveAll(o => !IsValidEmail(o));
                // Add the rest of the email addresses to the list.
                ccAdd.AddRange(ccitems.Select(o => new MailAddress(o)));
            }

            // if the email address is not correct and no other address can be send
            // ,then throw the exception.
            if (toAdd.Count == 0)
            {
                _ = errorMailAddress.AppendLine("* 寄件者信箱格式錯誤");
                throw new InvalidOperationException(string.Join(Environment.NewLine, errorMailAddress));
            }

            if (errorSendAdrress.Length > 1)
            {
                MailMessage msg = new MailMessage();
                msg.From = new MailAddress(fromAddress);
                msg.To.Add(new MailAddress(_option.Value.MailOwner));
                msg.Subject = "Email格式錯誤通知";
                msg.Body = toAddress;
                _ = ToSend(msg);
            }

            if (toAdd.Count > 0)
            {
                MailMessage msg = new MailMessage();
                msg.From = fromAdd;
                foreach (MailAddress o in toAdd)
                {
                    msg.To.Add(o);
                }
                foreach (MailAddress o in ccAdd)
                {
                    msg.CC.Add(o);
                }
                msg.Subject = mailSubject;
                msg.Body = mailBody;

                _ = ToSend(msg);
            }
        }

        #region private method

        private bool ToSend(MailMessage mail)
        {
            bool isMailSented = false;
            if (client == null)
            {
                throw new ArgumentException("Mail 參數尚未指定或尚未初始");
            }
            client.SendCompleted += new SendCompletedEventHandler((object sender, AsyncCompletedEventArgs e) =>
            {
                string? token = e.UserState?.ToString();
                if (e.Error != null)
                {
                    client.SendCompleted += new SendCompletedEventHandler(
                        (object sender, AsyncCompletedEventArgs e) =>
                        {
                            string? token = e.UserState?.ToString();
                            if (e.Error != null)
                            {
                                //to log
                                System.Diagnostics.Debug.WriteLine($"[{token}] \n {e.Error}");
                            }
                            else
                            {
                                //to log
                                System.Diagnostics.Debug.WriteLine("Message sent.");
                                isMailSented = true;
                            }
                        }
                    );
                    //to log
                    System.Diagnostics.Debug.WriteLine($"[{token}] \n {e.Error}");
                }
                else
                {
                    //to log
                    System.Diagnostics.Debug.WriteLine("Message sent.");
                    isMailSented = true;
                }
            });
            client.Send(mail);
            return isMailSented;
        }
        /// <summary>
        /// check the email address is valid or not
        /// </summary>
        /// <param name="mailAddress"></param>
        /// <returns></returns>
        private bool IsValidEmail(string mailAddress)
        {
            //return Regex.IsMatch(mailAddress.Trim(),
            //       @"^(?("")("".+?""@)|(([0-9a-zA-Z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-zA-Z])@))" +
            //       @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,6}))$");
            try
            {
                MailAddress addr = new MailAddress(mailAddress);
                return addr.Address == mailAddress;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// create mail address
        /// </summary>
        /// <param name="address"></param>
        /// <param name="name"></param>
        /// <param name="errorMessage"></param>
        /// <param name="errorMessages"></param>
        /// <returns></returns>
        private MailAddress? CreateMailAddress(string address, string name, string errorMessage, List<string> errorMessages)
        {
            if (string.IsNullOrEmpty(address) || string.IsNullOrEmpty(name))
            {
                return null;
            }

            address = address.Trim();

            if (!IsValidEmail(address))
            {
                errorMessages.Add($"* {errorMessage}");
                return null;
            }

            return new MailAddress(address, name);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
