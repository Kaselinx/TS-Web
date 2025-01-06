using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TSL.TAAA.Service.Interface
{
    public interface IPOTService
    {
        /// <summary>
        /// log in to AD using user name and password
        /// </summary>
        /// <param name="UserId">user id</param>
        /// <param name="Password">user name    </param>
        /// <param name="OTPPasswd">password</param>
        /// <returns></returns>
        XmlNode Login(string UserId, string Password, string OTPPasswd);

    }
}
