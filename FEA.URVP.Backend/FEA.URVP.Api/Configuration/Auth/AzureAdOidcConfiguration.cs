using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace FEA.URVP.Api.Configuration.Auth;

/// <summary>
/// Azure AD (AUB SSO) OpenID Connect handler configuration.
/// </summary>
public static class AzureAdOidcConfiguration
{
    /// <summary>
    /// The settings without which the handler cannot be built.
    /// </summary>
    private static readonly string[] RequiredKeys = ["AzureAd:TenantId", "AzureAd:ClientId"];

    /// <summary>
    /// Names the required settings that are absent.
    /// </summary>
    /// <remarks>
    /// The OIDC handler is an <see cref="Microsoft.AspNetCore.Authentication.IAuthenticationRequestHandler"/>,
    /// so ASP.NET Core resolves its options on every request to check for the callback path. A
    /// missing setting discovered inside the options factory therefore fails every request in the
    /// application — including <c>/health/live</c> — rather than just the sign-in route, and it
    /// does so long after startup has reported success. Callers use this to decide whether the
    /// scheme can be registered at all, which keeps the failure at boot where it belongs.
    /// </remarks>
    public static IReadOnlyList<string> MissingSettings(IConfiguration configuration) =>
        RequiredKeys.Where(key => string.IsNullOrWhiteSpace(configuration[key])).ToArray();

    public static bool IsConfigured(IConfiguration configuration) =>
        MissingSettings(configuration).Count == 0;

    public static void Configure(
        OpenIdConnectOptions options,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var missing = MissingSettings(configuration);
        if (missing.Count > 0)
        {
            throw new InvalidOperationException(
                $"Azure AD authentication is missing required settings: {string.Join(", ", missing)}.");
        }

        var instance = configuration["AzureAd:Instance"] ?? "https://login.microsoftonline.com/";
        var tenantId = configuration["AzureAd:TenantId"]!;
        var clientId = configuration["AzureAd:ClientId"]!;
        var clientSecret = configuration["AzureAd:ClientSecret"];

        var authority = $"{instance.TrimEnd('/')}/{tenantId}/v2.0";

        options.Authority = authority;
        options.MetadataAddress = $"{authority}/.well-known/openid-configuration";
        options.ClientId = clientId;
        options.CallbackPath = configuration["AzureAd:CallbackPath"] ?? "/signin-oidc-ad";
        options.SignInScheme = AuthenticationConfiguration.CookieScheme;

        // Metadata must be fetched over TLS everywhere except local development, where a
        // developer may be behind a TLS-terminating corporate proxy.
        options.RequireHttpsMetadata = !environment.IsDevelopment();

        ConfigureBackchannel(options, configuration, environment);
        ConfigureFlow(options, clientSecret);
        ConfigureCorrelationCookies(options);

        if (configuration.GetValue("AzureAd:ForceAccountSelection", false))
        {
            options.Prompt = "select_account";
        }

        // The frontend is a static export with no token handling of its own; nothing downstream
        // needs a token, so none is persisted into the session cookie.
        options.SaveTokens = false;
        options.GetClaimsFromUserInfoEndpoint = false;

        // The session lifetime is owned by Auth:Cookie:ExpireHours, not by the id_token's exp.
        options.UseTokenLifetime = false;

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");

        options.TokenValidationParameters = BuildTokenValidation(clientId, instance, tenantId);
        options.Events = OidcEventHandlers.CreateAzureAdOidcEvents(configuration);
    }

    /// <summary>
    /// Selects the response type.
    /// </summary>
    /// <remarks>
    /// Authorization code with PKCE is used whenever <c>AzureAd:ClientSecret</c> is configured,
    /// and is the preferred flow.
    /// <para>
    /// Without a secret the handler falls back to <c>response_type=id_token</c> with
    /// <c>response_mode=form_post</c>. Rationale: the AUB app registration is a public client
    /// with no secret or certificate, and Azure AD will not complete a server-side code exchange
    /// for a confidential client without one. This application needs identity only — it calls no
    /// downstream Microsoft API — so no access or refresh token is ever requested. The id_token
    /// is POSTed directly to the server (never placed in a URL fragment, never seen by
    /// JavaScript) and is validated against the tenant signing keys with a one-time nonce. The
    /// residual weakness relative to code+PKCE is the absence of proof-of-possession binding on
    /// the authorization response, which the correlation and nonce cookies mitigate.
    /// </para>
    /// To move to code+PKCE, add a secret or certificate to the app registration and set
    /// <c>AzureAd:ClientSecret</c>; no code change is required.
    /// </remarks>
    private static void ConfigureFlow(OpenIdConnectOptions options, string? clientSecret)
    {
        options.ResponseMode = OpenIdConnectResponseMode.FormPost;

        if (!string.IsNullOrWhiteSpace(clientSecret))
        {
            options.ClientSecret = clientSecret;
            options.ResponseType = OpenIdConnectResponseType.Code;
            options.UsePkce = true;
            return;
        }

        options.ResponseType = OpenIdConnectResponseType.IdToken;
        options.UsePkce = false;
    }

    /// <summary>
    /// <c>response_mode=form_post</c> means the identity provider submits a cross-site POST back
    /// to the callback, so the correlation and nonce cookies must survive a cross-site request.
    /// SameSite=None is the only mode browsers send in that case, and it mandates Secure.
    /// </summary>
    private static void ConfigureCorrelationCookies(OpenIdConnectOptions options)
    {
        options.CorrelationCookie.SameSite = SameSiteMode.None;
        options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;
        options.CorrelationCookie.HttpOnly = true;
        options.CorrelationCookie.IsEssential = true;

        options.NonceCookie.SameSite = SameSiteMode.None;
        options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;
        options.NonceCookie.HttpOnly = true;
        options.NonceCookie.IsEssential = true;
    }

    private static void ConfigureBackchannel(
        OpenIdConnectOptions options,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        // Certificate validation is never relaxed outside Development, and even there it must be
        // opted into explicitly for a TLS-inspecting corporate proxy.
        var allowInvalidCertificate =
            environment.IsDevelopment()
            && configuration.GetValue("AzureAd:AllowInvalidBackchannelCertificate", false);

        var handler = new HttpClientHandler { UseProxy = true };

        if (allowInvalidCertificate)
        {
            handler.ServerCertificateCustomValidationCallback = static (_, _, _, _) => true;
        }

        options.BackchannelHttpHandler = handler;
    }

    private static TokenValidationParameters BuildTokenValidation(
        string clientId,
        string instance,
        string tenantId)
    {
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidAudience = clientId,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            RequireSignedTokens = true,
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.FromMinutes(2),
            NameClaimType = "preferred_username",
            RoleClaimType = "role"
        };

        // Pinning the issuer is only correct for a single-tenant registration. The multi-tenant
        // authorities ("common", "organizations", "consumers") mint per-tenant issuers, so those
        // are left to metadata-driven validation.
        if (Guid.TryParse(tenantId, out _))
        {
            parameters.ValidIssuer = $"{instance.TrimEnd('/')}/{tenantId}/v2.0";
        }

        return parameters;
    }
}
