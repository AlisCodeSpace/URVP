using FEA.URVP.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace FEA.URVP.Tests.Security;

public sealed class SqlConnectionStringTests
{
    [Fact]
    public void Production_without_trust_opt_in_forces_encrypt_and_validates_the_certificate()
    {
        var normalized = SqlConnectionString.Normalize(
            "Server=db.example.com;Database=FEA_URVP;User Id=u;Password=p",
            allowTrustServerCertificate: false);

        Assert.False(SqlConnectionString.RequestsTrustServerCertificate(normalized));
        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(normalized);
        Assert.True(builder.Encrypt);
        Assert.True(builder.MultipleActiveResultSets);
        Assert.False(builder.TrustServerCertificate);
    }

    [Fact]
    public void Production_keeps_trust_server_certificate_when_the_connection_string_opts_in()
    {
        var production = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "Production"
            })
            .Build();

        var normalized = SqlConnectionString.Normalize(
            "data source=provost-urvp.mssql.somee.com;initial catalog=provost-urvp;user id=u;pwd=p;TrustServerCertificate=True",
            production);

        var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(normalized);
        Assert.True(builder.Encrypt);
        Assert.True(builder.TrustServerCertificate);
        Assert.Equal("provost-urvp.mssql.somee.com", builder.DataSource);
        Assert.Equal("provost-urvp", builder.InitialCatalog);
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
    public void Development_or_explicit_config_allows_trusting_any_certificate()
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
        var productionOptIn = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "Production",
                [SqlConnectionString.AllowTrustServerCertificateKey] = "true"
            })
            .Build();

        Assert.True(SqlConnectionString.AllowsTrustServerCertificate(development));
        Assert.False(SqlConnectionString.AllowsTrustServerCertificate(production));
        Assert.True(SqlConnectionString.AllowsTrustServerCertificate(productionOptIn));
    }
}
