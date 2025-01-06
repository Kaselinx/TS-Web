
using System.ServiceModel;
//using TSL.TAAA.Model.Service.OTP;

namespace TSL.TAAA.Service.OPTWebService
{
    [ServiceContract]
    public interface IOTPWS
    {
        [OperationContract]
        string HelloOTPService(string name);

        //[OperationContract]
        //PersonContract GetPerson(int personId);

    }
}
