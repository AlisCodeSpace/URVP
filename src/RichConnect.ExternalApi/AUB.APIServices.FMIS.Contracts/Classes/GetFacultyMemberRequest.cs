using System.Runtime.Serialization;

namespace AUB.APIServices.FMIS.Contracts.Classes;

[DataContract]
public class GetFacultyMemberRequest
{
    [DataMember (Order = 1)]
    public string MemberId { get; set; } = string.Empty;
}