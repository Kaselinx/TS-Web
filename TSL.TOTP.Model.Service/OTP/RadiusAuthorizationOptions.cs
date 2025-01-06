using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSL.Common.Model.Service.OTP
{
    public  class RadiusAuthorizationOptions
    {
        /// <summary>
        /// Primary Radius Server
        /// </summary>
        public required string RadiusServerPrimary { get; set; }

        /// <summary>
        /// Secondary Radius Server
        /// </summary>
        public required string RadiusServerSecondary { get; set; }

        /// <summary>
        ///   secret for radius server primary
        /// </summary>
        public required string RadiusSecretPrimary { get; set; }

        /// <summary>
        ///    secret for radius server secondary 
        /// </summary>
        public required string RadiusSecretSecondary { get; set; }

        /// <summary>
        ///  port for radius server primary
        /// </summary>
        public required int[] RadiusServerPortPrimary { get; set; }

        /// <summary>
        ///     port for radius server secondary
        /// </summary>
        public required int[] RadiusServerPortSecondary { get; set; }
    }
}
