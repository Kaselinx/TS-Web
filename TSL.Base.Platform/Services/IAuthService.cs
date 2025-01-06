using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSL.Base.Platform.Services
{
    public interface IAuthService
    {
        /// <summary>
        /// AD 驗證
        /// </summary>
        /// <param name="username">使用者名稱</param>
        /// <param name="password">使用者密碼</param>
        /// <returns></returns>
        bool ADValidateCredentials(string username, string password);
    }
}
