using System.ServiceModel;
using TSL.TAAA.Service.Interface;

namespace TSL.TAAS.Service.POT
{
    [ServiceContract]
    public class POTService : IPOTService
    {
        public string HelloPOTService(string name)
        {
            return "Hello " + name + " from POT Service!";
        }
    }
}
