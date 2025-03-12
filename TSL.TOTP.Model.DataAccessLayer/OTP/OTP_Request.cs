using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSL.Common.Model.DataAccessLayer.OTP
{
    public class OTP_Request
    {
        public required string SystemID { get; set; }
        public required string Tel_No { get; set; }
        public required string Mail { get; set; }
        public int Effect_Second { get; set; }
        public required string FunctionKey { get; set; }
    }
}
