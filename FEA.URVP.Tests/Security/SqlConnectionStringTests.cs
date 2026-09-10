using FEA.URVP.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace FEA.URVP.Tests.Security;

public sealed class SqlConnectionStringTests
{
    [Fact]
    public void Production_strips_trust_server_certificate_and_forces_encrypt()
    {
        var normalized = SqlConnectionString.Normalize(
            "Server=db.example.com;Database=FEA_URVP;User Id=u;Password=p;TrustServerCertificate=true",
            allowTrustServerCertificate: false);

        Assert.False(SqlConnectionString.RequestsTrustServerCertificate(normalized));
        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(normalized);
        Assert.True(builder.Encrypt);
        Assert.True(builder.MultipleActiveResultSets);
        Assert.False(builder.TrustServerCertificate);
    }

    [Fact]
    public void Development_keeps_trust_server_certificate_for_localdb()
    {
        var normalized = SqlConnectionString.Normalize(
            "Server=(localdb)\\MSSQLLocalDB;Database=FEA_URVP_Dev;Encrypt=True;TrustServerCertificate=True",
            allowTrustServerCertificate: true);

        Assert.True(SqlConnectionString.RequestsTrustServerCertificate(normalized));
    }

    [Fact]
    public void Prefixes_a_bare_host_copy_from_a_PaaS_dashboard()
    {
        var normalized = SqlConnectionString.Normalize(
            "sql.example.com,1433;Database=FEA_URVP;User Id=u;Password=p",
            allowTrustServerCertificate: false);

        Assert.Contains("sql.example.com,1433", normalized, StringComparison.OrdinalIgnoreCase);
        Assert.False(SqlConnectionString.RequestsTrustServerCertificate(normalized));
    }

    [Fact]
    public void Development_environment_is_the_only_one_allowed_to_trust_any_certificate()
    {
        var development = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "Development"
            })
            .Build();
        var production = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "Production"
            })
            .Build();

        Assert.True(SqlConnectionString.AllowsTrustServerCertificate(development));
        Assert.False(SqlConnectionString.AllowsTrustServerCertificate(production));
    }
}
