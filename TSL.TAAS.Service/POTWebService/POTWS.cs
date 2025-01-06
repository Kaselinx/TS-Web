using System.ServiceModel;
using TSL.TAAA.Service.POTWebService;

namespace TSL.TAAA.Service.POTWebService
{
    [ServiceContract]
    public class POTWS : IPOTWS
    {
        public string HelloPOTService(string name)
        {
            return "Hello " + name + " from POT Service!";
        }
    }
}
