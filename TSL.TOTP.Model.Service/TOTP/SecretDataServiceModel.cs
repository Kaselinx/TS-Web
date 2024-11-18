
using Dapper.Contrib.Extensions;
using System.ComponentModel.DataAnnotations.Schema;

namespace TSL.Common.Model.Service.TOTP
{
    /// <summary>
    /// The issuer of the TOTP (Time-based One-Time Password) 2FA (Two-Factor Authentication) token.
    /// </summary>
    public class SecretDataServiceModel
    {


        /// <summary>
        /// Secret Id as primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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
        /// is currently active or not
        /// </summary>
        public bool isActive { get; set; }

        /// <summary>
        /// Create_time
        /// </summary>
        public DateTime Create_Time { get; set; }


        /// <summary>
        /// Create_time
        /// </summary>
        public DateTime Updated_Time { get; set; }

    }
}
