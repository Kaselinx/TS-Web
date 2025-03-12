using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSL.Common.Model.DataAccessLayer.OTP
{
    public class OTP_Response
    {
        public bool Result { get; set; }
        public required string Msg { get; set; }
    }
}
