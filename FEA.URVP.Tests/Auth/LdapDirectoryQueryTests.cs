using FEA.URVP.Domain.Enums;
using FEA.URVP.Infrastructure.Ldap;

namespace FEA.URVP.Tests.Auth;

public sealed class LdapDirectoryQueryTests
{
    [Fact]
    public void Searches_sAMAccountName_then_UPN_then_mail_before_alias_domains()
    {
        var sequence = LdapDirectoryQuery.BuildUserLookupSequence(
            "aa624",
            "aa624@mail.aub.edu",
            "aa624@mail.aub.edu",
            ["aa624@aub.edu.lb", "aa624@mail.aub.edu"]);

        Assert.Equal("sAMAccountName", sequence[0].Attribute);
        Assert.Equal("aa624", sequence[0].Value);
        Assert.Equal("userPrincipalName", sequence[1].Attribute);
        Assert.Equal("aa624@mail.aub.edu", sequence[1].Value);
        Assert.Equal("mail", sequence[2].Attribute);
        Assert.Equal(("userPrincipalName", "aa624@aub.edu.lb"), sequence[3]);
        Assert.Equal(1, sequence.Count(item =>
            item.Attribute == "mail"
            && item.Value.Equals("aa624@mail.aub.edu", StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void User_filter_requires_a_user_object_not_a_contact()
    {
        var filter = LdapDirectoryQuery.UserObjectFilter("mail", "aa624@mail.aub.edu");

        Assert.Contains("(objectClass=user)", filter);
        Assert.Contains("(mail=aa624@mail.aub.edu)", filter);
        Assert.DoesNotContain("(|", filter);
    }

    [Fact]
    public void Nested_member_filter_uses_the_in_chain_matching_rule()
    {
        var filter = LdapDirectoryQuery.NestedMemberFilter("CN=aa624,OU=Students,DC=aub,DC=edu,DC=lb");

        Assert.StartsWith($"(member:{LdapDirectoryQuery.NestedMemberOid}:=", filter);
        Assert.Contains("CN=aa624,OU=Students,DC=aub,DC=edu,DC=lb", filter);
    }

    [Fact]
    public void Escapes_LDAP_filter_metacharacters_in_DNs()
    {
        var filter = LdapDirectoryQuery.NestedMemberFilter(@"CN=Foo),OU=Bar,DC=aub,DC=edu,DC=lb");

        Assert.Contains(@"CN=Foo\29,OU=Bar,DC=aub,DC=edu,DC=lb", filter);
        Assert.DoesNotContain("CN=Foo),", filter);
    }

    [Fact]
    public void Faculty_membership_wins_when_both_groups_match()
        => Assert.Equal(UserRole.Faculty, LdapDirectoryQuery.RoleFromMembership(true, true));

    [Fact]
    public void Direct_memberOf_CNs_map_student_when_faculty_is_absent()
    {
        var role = LdapDirectoryQuery.RoleFromGroupCns(
            ["Domain Users", "Students-STD", "Other"],
            "ALLACADstaff-STF",
            "Students-STD");

        Assert.Equal(UserRole.Student, role);
    }

    [Fact]
    public void Extracts_the_CN_from_a_group_DN()
        => Assert.Equal(
            "ALLACADstaff-STF",
            LdapDirectoryQuery.ExtractCnFromDn("CN=ALLACADstaff-STF,OU=Groups,DC=aub,DC=edu,DC=lb"));
}
