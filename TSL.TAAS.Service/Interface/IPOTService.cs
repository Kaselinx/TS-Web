
using System.ServiceModel;


namespace TSL.TAAA.Service.Interface
{
    [ServiceContract]
    public interface IPOTService
    {
        [OperationContract]
        string HelloPOTService(string name);
    }
}
