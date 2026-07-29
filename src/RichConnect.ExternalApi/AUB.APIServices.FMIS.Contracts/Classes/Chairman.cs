using System.Runtime.Serialization;

namespace AUB.APIServices.FMIS.Contracts.Classes;

[DataContract]
public class Chairman
{
    [DataMember (Order = 1)]
    public string MemberId { get; set; } = string.Empty;

    [DataMember (Order = 2)]
    public string Firstname { get; set; } = string.Empty;
    
    [DataMember (Order = 3)]
    public string Lastname { get; set; } = string.Empty;
    
    [DataMember (Order = 4)]
    public string Email { get; set; } = string.Empty;
    
    [DataMember (Order = 5)]
    public string Department { get; set; } = string.Empty;
}