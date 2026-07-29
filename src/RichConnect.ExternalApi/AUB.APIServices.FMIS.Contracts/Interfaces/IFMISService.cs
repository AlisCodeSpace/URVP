using System.ServiceModel;
using AUB.APIServices.FMIS.Contracts.Classes;
using ProtoBuf.Grpc;

namespace AUB.APIServices.FMIS.Contracts.Interfaces;

[ServiceContract]
public interface IFMISService
{
    [OperationContract] 
    Task<FacultyMemberLite[]> GetFacultyMembersInFaculty(GetFacultyMembersRequest request, CallContext context = default);
    
    [OperationContract] 
    Task<FacultyMember> GetFacultyMember(GetFacultyMemberRequest request, CallContext context = default);
}