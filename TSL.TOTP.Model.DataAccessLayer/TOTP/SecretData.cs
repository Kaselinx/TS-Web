using Dapper.Contrib.Extensions;
namespace TSL.Common.Model.DataAccessLayer.TOTP
{
    /// <summary>
    /// The issuer of the TOTP (Time-based One-Time Password) 2FA (Two-Factor Authentication) token.
    /// </summary>
    [Table("SecretData")]
    public class SecretData
    {
        /// <summary>
        /// Secret Id as primary key
        /// </summary>
        [Key]
        public int SecretId { get; set; }

        /// <summary>
        /// The issuer of the TOTP (Time-based One-Time Password) 2FA (Two-Factor Authentication) token.  this is actally Employee Number from active directory
        /// </summary>
        public required string EmployeeId { get; set; }

        /// <summary>
        /// The label associated with the TOTP 2FA token, typically the user's account name.
        /// </summary>
        public required string Label { get; set; }

        /// <summary>
        /// The secret key used to generate the TOTP 2FA token.
        /// </summary>
        public required string Secret { get; set; }


        /// <summary>
        /// creted time 
        /// </summary>

        public DateTime Create_Time { get; set; }

        /// <summary>
        /// last update time
        /// </summary>
        public DateTime Updated_Time { get; set; }

        /// <summary>
        /// is currently active or not
        /// </summary>
        public bool isActive { get; set; }
    }
}
