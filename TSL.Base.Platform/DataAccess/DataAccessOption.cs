

namespace TSL.Base.Platform.DataAccess
{
    /// <summary>
    /// DataAccessLayer Config object
    /// </summary>
    public class DataAccessOption
    {
        /// <summary>
        /// Primary Connection
        /// </summary>
        public string? ConnectionStringPrimary { get; set; }

        /// <summary>
        /// Read Only Connection..if there is any read only connection
        /// </summary>
        public string? ConnectionStringSecondary { get; set; }

        /// <summary>
        /// Connection Timeout
        /// </summary>
        public int ConnetionTimeout { get; set; }


        /// <summary>
        /// windows credential
        /// </summary>
        public string? Credential { get; set; }
    }
}
