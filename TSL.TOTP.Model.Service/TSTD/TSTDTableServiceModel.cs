

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TSL.Common.Model.Service.TSTD
{
    public class TSTDTableServiceModel
    {
        /// <summary>
        /// Key as Identity key, which means you don't need to insert this value when you insert a new record.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TSTDTableId { get; set; }

        /// <summary>
        /// creted time 
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// user name 
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// message
        /// </summary>
        public string Message { get; set; }
    }
}
