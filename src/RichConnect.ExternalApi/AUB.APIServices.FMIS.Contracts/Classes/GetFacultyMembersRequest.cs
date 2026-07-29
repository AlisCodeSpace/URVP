using System.Runtime.Serialization;

namespace AUB.APIServices.FMIS.Contracts.Classes;

[DataContract]
public class GetFacultyMembersRequest
{
    [DataMember (Order = 1)]
    public string Faculty { get; set; } = string.Empty;
}