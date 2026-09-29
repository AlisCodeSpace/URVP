using FEA.URVP.Domain.Entities.Semesters;

namespace FEA.URVP.Tests.Semesters;

public sealed class SemesterPhaseTests
{
    private static readonly DateTime Now = new(2026, 9, 29, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Ending_a_cycle_closes_registration_and_applications()
    {
        var semester = Running();
        semester.ApplyRegistrationWindow(Now.AddDays(-2), Now.AddDays(2), Now);
        semester.ApplyApplicationWindow(Now.AddDays(3), Now.AddDays(6), Now);

        semester.EndCycleNow(Now);

        Assert.False(semester.IsCycleActive(Now));
        Assert.False(semester.IsRegistrationWindowOpen(Now));
        Assert.False(semester.IsApplicationWindowOpen(Now));
    }

    [Fact]
    public void Registration_opens_only_while_the_cycle_is_active()
    {
        var semester = Running();
        semester.ApplyRegistrationWindow(Now.AddHours(-1), Now.AddDays(2), Now);

        Assert.True(semester.IsRegistrationWindowOpen(Now));
        Assert.False(semester.IsApplicationWindowOpen(Now));

        semester.EndCycleNow(Now);
        Assert.False(semester.IsRegistrationWindowOpen(Now));
    }

    [Fact]
    public void Application_opens_only_while_the_cycle_is_active()
    {
        var semester = Running();
        semester.ApplyApplicationWindow(Now.AddHours(-1), Now.AddDays(2), Now);

        Assert.True(semester.IsApplicationWindowOpen(Now));
        Assert.False(semester.IsRegistrationWindowOpen(Now));

        semester.EndCycleNow(Now);
        Assert.False(semester.IsApplicationWindowOpen(Now));
    }

    [Fact]
    public void Scheduled_windows_that_overlap_are_detected()
    {
        Assert.True(Semester.RangesOverlap(
            Now, Now.AddDays(3), Now.AddDays(1), Now.AddDays(4)));
        Assert.False(Semester.RangesOverlap(
            Now, Now.AddDays(1), Now.AddDays(1), Now.AddDays(2)));
    }

    private static Semester Running() => new()
    {
        IsActive = true,
        CycleStart = Now.AddDays(-5),
        CycleEnd = Now.AddDays(40),
    };
}
