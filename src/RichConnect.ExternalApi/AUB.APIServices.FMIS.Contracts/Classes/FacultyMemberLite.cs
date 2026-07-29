using System.Runtime.Serialization;

namespace AUB.APIServices.FMIS.Contracts.Classes;

[DataContract]
public class FacultyMemberLite
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
    public string Rank { get; set; } = string.Empty;
    
    [DataMember (Order = 6)]
    public string Department { get; set; } = string.Empty;
    
    [DataMember (Order = 7)]
    public string Faculty { get; set; } = string.Empty;
    
    [DataMember (Order = 8)]
    public string FTE { get; set; } = string.Empty;
}