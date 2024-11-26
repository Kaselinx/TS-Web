using Dapper.Contrib.Extensions;

namespace TSL.Common.Model.DataAccessLayer.TSTD
{

    /// <summary>
    /// DataAccessModel for TSTDTable, should be same as the table name in the database.
    /// </summary>
    [Table("TSTDTable")]
    public class TSTDTable
    {
        /// <summary>
        /// Key as Identity key, which means you don't need to insert this value when you insert a new record.
        /// </summary>
        [Key]
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
