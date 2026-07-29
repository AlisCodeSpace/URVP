using System.Runtime.Serialization;

namespace AUB.APIServices.FMIS.Contracts.Classes;

[DataContract]
public class BioData
{
    [DataMember(Order = 1)]
    public string ShortBio { get; set; } = string.Empty;
    
    [DataMember(Order = 2)]
    public string TeachingInterests { get; set; } = string.Empty;
    
    [DataMember(Order = 3)]
    public string BasicResearchInterests { get; set; } = string.Empty;
    
    [DataMember(Order = 4)]
    public string ClinicalResearchInterests { get; set; } = string.Empty;
    
    [DataMember(Order = 5)]
    public string Skills { get; set; } = string.Empty;
    
    [DataMember(Order = 6)]
    public string Remark { get; set; } = string.Empty;
}