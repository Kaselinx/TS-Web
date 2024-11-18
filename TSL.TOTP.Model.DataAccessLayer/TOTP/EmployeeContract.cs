using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TSL.Common.Model.DataAccessLayer.TOTP
{
    [Table("EmployeeContract")]
    public class EmployeeContract
    {
        /// <summary>
        /// this is actually employNo from Active directory
        /// </summary>
        [Key]
        public int EmployeeId { get; set; }

        /// <summary>
        /// employee nickname
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Employee Chinese Name. 
        /// </summary>
        public string ChinseName { get; set; } = string.Empty;
    }
}
