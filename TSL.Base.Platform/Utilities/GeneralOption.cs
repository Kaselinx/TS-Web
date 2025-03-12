

namespace TSL.Base.Platform.Utilities
{
    /// <summary>
    /// Settings for general purposed options
    /// </summary>
    public class GeneralOption
    {
        /// <summary>
        /// Option setting to indicates if Miniprofiler is enabled for the application
        /// </summary>
        public bool EnableMiniProfiler { get; set; }

        /// <summary>
        /// base dn
        /// </summary>
        public string DistinguishedName { get; set; }

        /// <summary>
        /// domain name
        /// </summary>
        public string DomainName { get; set; }

        /// <summary>
        /// role value
        /// </summary>
        public string SRRoleAttrName { get; set; }


        /// <summary>
        /// smtp server location
        /// </summary>
        public string SmtpServer { get; set; }

        /// <summary>
        /// mail sender address
        /// </summary>
        public string Mailsender { get; set; }

        /// <summary>
        /// last password change date
        /// </summary>
        public string SRPWDAttrDate { get; set; }


        /// <summary>
        /// mail server owner
        /// </summary>
        public string MailOwner { get; set; }
    }
}
