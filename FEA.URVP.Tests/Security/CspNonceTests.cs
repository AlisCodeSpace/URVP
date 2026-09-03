using FEA.URVP.Api.Middleware;
using Microsoft.AspNetCore.Http;

namespace FEA.URVP.Tests.Security;

public sealed class CspNonceTests
{
    [Fact]
    public void Get_returns_the_same_nonce_for_one_response()
    {
        // The header and the injected script tags must agree, so the value has to be cached.
        var context = new DefaultHttpContext();

        Assert.Equal(CspNonce.Get(context), CspNonce.Get(context));
    }

    [Fact]
    public void Each_response_receives_a_distinct_nonce()
    {
        var first = CspNonce.Get(new DefaultHttpContext());
        var second = CspNonce.Get(new DefaultHttpContext());

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Nonce_carries_at_least_128_bits_of_entropy()
    {
        var nonce = CspNonce.Get(new DefaultHttpContext());

        Assert.True(Convert.FromBase64String(nonce).Length >= 16);
    }

    [Fact]
    public void Peek_does_not_create_a_nonce()
    {
        var context = new DefaultHttpContext();

        Assert.Null(CspNonce.Peek(context));

        var created = CspNonce.Get(context);

        Assert.Equal(created, CspNonce.Peek(context));
    }
}
