
using System.ServiceModel;
//using TSL.TAAA.Model.Service.OTP;

namespace TSL.TAAA.Service.Interface
{
    [ServiceContract]
    public interface IOTPService
    {
        [OperationContract]
        string HelloOTPService(string name);

        //[OperationContract]
        //PersonContract GetPerson(int personId);

    }
}
