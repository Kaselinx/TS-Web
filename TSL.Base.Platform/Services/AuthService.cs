using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using TSL.Base.Platform.lnversionOfControl;
using TSL.Base.Platform.Utilities;
using DirectoryEntry = System.DirectoryServices.DirectoryEntry;

namespace TSL.Base.Platform.Services
{
    [RegisterIOC(IocType.Singleton)]
    public class AuthService : IAuthService
    {
        private readonly IOptions<GeneralOption> _option;


        /// <summary>
        /// constr  
        /// </summary>
        /// <param name="option"></param>
        public AuthService(IOptions<GeneralOption> option)
        {
            _option = option ?? throw new ArgumentNullException(nameof(option));
        }


        /// <summary>
        /// Validate the user credentials with AD
        /// </summary>
        /// <param name="username">user name</param>
        /// <param name="password">password</param>
        /// <returns></returns>
        public bool ADValidateCredentials(string username, string password)
        {
            string domainName = _option.Value.DomainName;
            string path = $"LDAP://{domainName}";

            try
            {
                using (PrincipalContext context = new PrincipalContext(ContextType.Domain, domainName, null, null))
                {
                    if (context.ValidateCredentials(username, password))
                    {
                        using (DirectoryEntry? de = new DirectoryEntry(path))
                        {
                            using (DirectorySearcher ds = new DirectorySearcher(de))
                            {
                                return true;
                            }
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }

        }
    }
}
