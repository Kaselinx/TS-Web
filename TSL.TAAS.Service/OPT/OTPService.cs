//using Bogus;
//using Microsoft.Extensions.Configuration.UserSecrets;
//using TSL.TAAA.Model.Service.OTP;
using TSL.TAAA.Service.Interface;

namespace TSL.TAAS.Service.OPT
{
    public class OTPService : IOTPService
    {
        public string HelloOTPService(string name)
        {
            return "Hello " + name + " from OTP Service!";
        }


        
        //public PersonContract GetPerson(int personId)
        //{
        //    var data = DataHelper.GeneratePersonData(100);
        //    return data.FirstOrDefault(x => x.Id == personId);
        //}
    }

    //public static class DataHelper
    //{
    //    public static Faker GetFakerInstance(string langIso2)
    //    {
    //        Faker faker = new(langIso2);
    //        return faker;
    //    }

    //    public static IList<PersonContract> GeneratePersonData(int count)
    //    {
    //        var faker = GetFakerInstance("tr");
    //        List<PersonContract> personContracts = [];
    //        for (int i = 0; i < count; i++)
    //        {
    //            personContracts.Add(new()
    //            {
    //                Id = faker.IndexFaker + i,
    //                Name = faker.Person.FirstName,
    //                Surname = faker.Person.LastName
    //            });
    //        }

    //        return personContracts;
    //    }
    //}
}
