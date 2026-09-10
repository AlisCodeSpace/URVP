using FEA.URVP.Application.Directory;
using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Tests.Auth;

public sealed class AubEmailDirectoryRoleTests
{
    [Theory]
    [InlineData("aa624@mail.aub.edu", UserRole.Student)]
    [InlineData("aa624@MAIL.AUB.EDU", UserRole.Student)]
    [InlineData("aa624@mail.aub.edu.lb", UserRole.Student)]
    [InlineData("faculty@aub.edu.lb", UserRole.Faculty)]
    public void Maps_known_AUB_mailbox_domains(string email, UserRole expected)
        => Assert.Equal(expected, AubEmailDirectoryRole.FromEmail(email));

    [Theory]
    [InlineData("someone@gmail.com")]
    [InlineData("not-an-email")]
    [InlineData("")]
    public void Leaves_unknown_addresses_unresolved(string email)
        => Assert.Null(AubEmailDirectoryRole.FromEmail(email));

    [Fact]
    public void Does_not_treat_student_mail_aub_edu_lb_as_faculty()
        => Assert.Equal(UserRole.Student, AubEmailDirectoryRole.FromEmail("name@mail.aub.edu.lb"));
}
