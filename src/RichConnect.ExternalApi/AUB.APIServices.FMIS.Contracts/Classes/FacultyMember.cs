using System.Runtime.Serialization;

namespace AUB.APIServices.FMIS.Contracts.Classes;

[DataContract]
public class FacultyMember
{
    [DataMember (Order = 1)]
    public string Id { get; set; } = string.Empty;
    
    [DataMember (Order = 2)]
    public string MemberId { get; set; } = string.Empty;
    
    [DataMember (Order = 3)]
    public string PayrollNumber { get; set; } = string.Empty;
    
    [DataMember (Order = 4)]
    public string Firstname { get; set; } = string.Empty;
    
    [DataMember (Order = 5)]
    public string Lastname { get; set; } = string.Empty;
    
    [DataMember (Order = 6)]
    public string Email { get; set; } = string.Empty;
    
    [DataMember (Order = 7)]
    public string Rank { get; set; } = string.Empty;
    
    [DataMember (Order = 8)]
    public string Department { get; set; } = string.Empty;
    
    [DataMember (Order = 9)]
    public string FacultyCode { get; set; } = string.Empty;
    
    [DataMember (Order = 10)]
    public string Faculty { get; set; } = string.Empty;
    
    [DataMember (Order = 11)]
    public string FTE { get; set; } = string.Empty;
    
    [DataMember (Order = 12)]
    public string Extension { get; set; } = string.Empty;
    
    [DataMember (Order = 13)]
    public Chairman? Chairman { get; set; }
    
    [DataMember (Order = 14)]
    public Dean? Dean { get; set; }
    
    [DataMember (Order = 15)]
    public BioData? BioData { get; set; }
}