﻿
using System.Runtime.Serialization;


namespace TSL.TAAA.Model.Service.OTP
{
    [DataContract]
    public class PersonContract
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember] 
        public Guid UUId { get; set; } = Guid.NewGuid();

        [DataMember] 
        public string Name { get; set; } = string.Empty;

        [DataMember] 
        public string Surname { get; set; } = string.Empty;
    }
}
