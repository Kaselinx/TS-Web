

namespace TSL.Base.Platform.Utilities
{
    public interface IMailUtil
    {
        void SendMail(string fromAddress, string fromName, string toAddress, string toName,
                      string ccAddress, string ccName, string mailSubject, string mailBody);

        void SendMail(string fromAddress, string toAddress, string ccAddress, string mailSubject, string mailBody);
    }
}
