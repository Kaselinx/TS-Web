using System.ComponentModel.DataAnnotations;

namespace TSL.TSTD.API.Model
{
    public class TSTDTableViewModel
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
        [Required(ErrorMessage = "請入名稱", AllowEmptyStrings = false)]
        [RegularExpression(@"^[A-Za-z0-9]+$", ErrorMessage = "只允許輸入英數字")]
        [MaxLength(20, ErrorMessage = "名稱最長20碼")]
        public required string Username { get; set; }

        /// <summary>
        /// message
        /// </summary>
        public string? Message { get; set; }
    }
}
