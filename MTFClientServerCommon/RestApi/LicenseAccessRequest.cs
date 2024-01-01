using System.Runtime.Serialization;

namespace MTFClientServerCommon.RestApi
{
    [DataContract]
    public class LicenseAccessRequest
    {
        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public string Reason { get; set; }

        [DataMember]
        public string HwId { get; set; }

        [DataMember]
        public string Creator { get; set; }
    }
}