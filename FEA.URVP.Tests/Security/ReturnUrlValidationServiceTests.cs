using FEA.URVP.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace FEA.URVP.Tests.Security;

public sealed class ReturnUrlValidationServiceTests
{
    private const string RequestOrigin = "https://urvp.aub.edu.lb";

    private static ReturnUrlValidationService Service(
        IWebHostEnvironment? environment = null,
        IConfiguration? configuration = null)
    {
        var context = new DefaultHttpContext();
        context.Request.Scheme = "https";
        context.Request.Host = new HostString("urvp.aub.edu.lb");

        var accessor = new HttpContextAccessor { HttpContext = context };

        return new ReturnUrlValidationService(
            configuration ?? TestEnvironments.Config(),
            accessor,
            environment ?? TestEnvironments.Production,
            NullLogger<ReturnUrlValidationService>.Instance);
    }

    [Theory]
    [InlineData("/auth/callback")]
    [InlineData("/my-projects?user=abc")]
    [InlineData("/")]
    public void Root_relative_paths_are_accepted(string returnUrl)
    {
        Assert.Equal(returnUrl, Service().ValidateReturnUrl(returnUrl));
    }

    [Theory]
    [InlineData("//evil.example/callback")]
    [InlineData("/\\evil.example/callback")]
    [InlineData("https://evil.example/callback")]
    [InlineData("http://urvp.aub.edu.lb/callback")]
    [InlineData("javascript:alert(1)")]
    [InlineData("not a url")]
    public void Off_origin_and_malformed_return_urls_fall_back_to_the_default(string returnUrl)
    {
        Assert.Equal("/", Service().ValidateReturnUrl(returnUrl));
    }

    [Fact]
    public void Absolute_url_matching_the_request_origin_is_accepted()
    {
        var returnUrl = $"{RequestOrigin}/auth/callback";

        Assert.Equal(returnUrl, Service().ValidateReturnUrl(returnUrl));
    }

    [Fact]
    public void Absolute_url_on_an_allow_listed_origin_is_accepted()
    {
        var configuration = TestEnvironments.WithCorsOrigins("https://portal.aub.edu.lb");

        var returnUrl = "https://portal.aub.edu.lb/auth/callback";

        Assert.Equal(returnUrl, Service(configuration: configuration).ValidateReturnUrl(returnUrl));
    }

    [Fact]
    public void Origin_prefix_confusion_is_rejected()
    {
        // A host that merely starts with the allow-listed name must not match.
        var configuration = TestEnvironments.WithCorsOrigins("https://portal.aub.edu.lb");

        var result = Service(configuration: configuration)
            .ValidateReturnUrl("https://portal.aub.edu.lb.evil.example/callback");

        Assert.Equal("/", result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Missing_return_url_falls_back_to_the_default(string? returnUrl)
    {
        Assert.Equal("/", Service().ValidateReturnUrl(returnUrl));
    }

    [Fact]
    public void Control_characters_that_could_split_the_location_header_are_rejected()
    {
        Assert.Equal("/", Service().ValidateReturnUrl("/callback\r\nSet-Cookie: a=b"));
    }

    [Fact]
    public void BuildFrontendUrl_stays_relative_in_production()
    {
        Assert.Equal("/auth/callback", Service().BuildFrontendUrl("/auth/callback"));
        Assert.Equal("/auth/callback", Service().BuildFrontendUrl("auth/callback"));
    }

    [Fact]
    public void BuildFrontendUrl_uses_the_allow_listed_origin_in_development()
    {
        var service = Service(
            TestEnvironments.Development,
            TestEnvironments.WithCorsOrigins("https://localhost:3000"));

        Assert.Equal("https://localhost:3000/auth/callback", service.BuildFrontendUrl("/auth/callback"));
    }
}
