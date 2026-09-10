using FEA.URVP.Application.Email;

namespace FEA.URVP.Tests.Email;

public sealed class SmtpSendErrorTests
{
    [Fact]
    public void Format_includes_host_and_inner_exception()
    {
        var inner = new InvalidOperationException(
            "No connection could be made because the target machine actively refused it.");
        var ex = new InvalidOperationException("Failure sending mail.", inner);

        var text = SmtpSendError.Format("smtp.aub.edu.lb", 25, ex);

        Assert.Equal(
            "smtp.aub.edu.lb:25: Failure sending mail. → No connection could be made because the target machine actively refused it.",
            text);
    }

    [Fact]
    public void Join_skips_blank_and_combines_host_errors()
    {
        var combined = SmtpSendError.Join(
            "localhost:25: Connection refused",
            null,
            "smtp.aub.edu.lb:25: Failure sending mail.");

        Assert.Equal(
            "localhost:25: Connection refused | smtp.aub.edu.lb:25: Failure sending mail.",
            combined);
    }

    [Fact]
    public void Join_returns_null_when_empty()
    {
        Assert.Null(SmtpSendError.Join(null, "  "));
    }
}
