
using System.ServiceModel;


namespace TSL.TAAA.Service.POTWebService
{
    [ServiceContract]
    public interface IPOTWS
    {
        [OperationContract]
        string HelloPOTService(string name);
    }
}
