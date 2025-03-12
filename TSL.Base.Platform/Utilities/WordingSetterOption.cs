
namespace TSL.Base.Platform.Utilities
{
    /// <summary>
    /// Wording setter option for Mail and SMS
    /// </summary>
    public class WordingSetterOption
    {
        public POTOTPMail? PotOTPMail { get; set; }

        public POTOTPSMS? PotOTPSms { get; set; }
    }

    /// <summary>
    /// Mail wording
    /// </summary>
    public class POTOTPMail
    {
        public string? Subject { get; set; }
        public string? Content { get; set; }
    }


    /// <summary>
    /// SMS wordiong
    /// </summary>
    public class POTOTPSMS
    {
        public string? Subject { get; set; }
        public string? Content { get; set; }
    }
}