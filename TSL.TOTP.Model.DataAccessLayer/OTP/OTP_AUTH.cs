using Dapper.Contrib.Extensions;

namespace TSL.Common.Model.DataAccessLayer.OTP
{
    [Table("OTP_AUTH")]
    public class OTP_AUTH
    {
        /// <summary>
        /// primary key
        /// </summary>
        [Key]
        public int SeqNo { get; set; }

        /// <summary>
        /// System identifier
        /// </summary>
        public required string SystemID { get; set; }

        // Telephone number
        public required string Tel_No { get; set; }

        // Email address
        public required string Mail { get; set; }

        // One-time password
        public required string OTP { get; set; }

        // Status of the OTP
        public required string Status { get; set; }

        // Creation time of the OTP
        public DateTime? CreateTime { get; set; }

        // End time of the OTP
        public DateTime? EndTime { get; set; }

        // Effective duration in seconds
        public int? Effect_Second { get; set; }

        // Status modification date
        public DateTime? Status_ModiDate { get; set; }

        // Source IP address
        public required string SourceIP { get; set; }
    }
}
