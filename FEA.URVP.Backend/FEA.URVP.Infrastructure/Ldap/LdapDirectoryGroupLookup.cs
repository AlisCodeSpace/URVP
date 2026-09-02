using System.DirectoryServices.Protocols;
using System.Net;
using FEA.URVP.Application.Abstractions.Directory;
using FEA.URVP.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FEA.URVP.Infrastructure.Ldap;

/// <summary>
/// Cross-platform LDAP lookup against AUB on-prem Active Directory.
/// On a domain-joined Windows host, Negotiate binds as the process identity
/// (same pattern as AUB's LdapService).
/// </summary>
public sealed class LdapDirectoryGroupLookup : IDirectoryGroupLookup
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<LdapDirectoryGroupLookup> _logger;

    public LdapDirectoryGroupLookup(
        IConfiguration configuration,
        ILogger<LdapDirectoryGroupLookup> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public UserRole? ResolveRole(string preferredUsername, string email)
    {
        if (!_configuration.GetValue("Ldap:Enabled", true))
        {
            _logger.LogInformation("LDAP directory lookup is disabled.");
            return null;
        }

        var samAccountName = StripDomain(
            !string.IsNullOrWhiteSpace(preferredUsername) ? preferredUsername : email);
        var upn = FirstNonEmpty(preferredUsername, email);
        var mail = FirstNonEmpty(email, preferredUsername);

        if (string.IsNullOrWhiteSpace(samAccountName)
            && string.IsNullOrWhiteSpace(upn)
            && string.IsNullOrWhiteSpace(mail))
        {
            _logger.LogWarning("LDAP lookup skipped: no sAMAccountName, UPN, or mail.");
            return null;
        }

        var server = _configuration["Ldap:Server"] ?? "win2k.aub.edu.lb";
        var configuredPort = _configuration.GetValue("Ldap:Port", 389);
        var ports = configuredPort == 3268
            ? new[] { 3268, 389 }
            : new[] { configuredPort, configuredPort == 389 ? 3268 : 389 };

        var configuredBaseDn = _configuration["Ldap:BaseDn"];
        var facultyGroup = _configuration["Ldap:FacultyGroupName"] ?? "ALLACADstaff-STF";
        var studentGroup = _configuration["Ldap:StudentGroupName"] ?? "Students-STD";

        try
        {
            SearchResultEntry? entry = null;
            string? usedServer = null;
            var usedPort = 0;
            string? usedBaseDn = null;

            foreach (var port in ports.Distinct())
            {
                using var connection = CreateConnection(server, port);
                var baseDn = string.IsNullOrWhiteSpace(configuredBaseDn)
                    ? DiscoverBaseDn(connection) ?? BuildBaseDn(server)
                    : configuredBaseDn;

                _logger.LogInformation(
                    "LDAP: connected to {Server}:{Port}. Searching under {BaseDn} for sAMAccountName={Sam}, UPN={Upn}, mail={Mail}.",
                    server,
                    port,
                    baseDn,
                    samAccountName,
                    upn,
                    mail);

                entry = FindUser(connection, baseDn, samAccountName, upn, mail);
                if (entry is not null)
                {
                    usedServer = server;
                    usedPort = port;
                    usedBaseDn = baseDn;
                    break;
                }

                _logger.LogWarning(
                    "LDAP: no user under {BaseDn} on {Server}:{Port}.",
                    baseDn,
                    server,
                    port);
            }

            if (entry is null)
            {
                _logger.LogWarning(
                    "LDAP: AD did not return a user object. Entra can still show {Upn} because cloud identity is not the same as an LDAP search. Role will fall back to the database.",
                    upn);
                return null;
            }

            var foundSam = Attr(entry, "sAMAccountName");
            var foundUpn = Attr(entry, "userPrincipalName");
            var foundMail = Attr(entry, "mail");
            var dn = entry.DistinguishedName;
            var groupCns = ReadMemberOfCns(entry);

            // Global Catalog (3268) only returns universal groups in memberOf.
            if (usedPort == 3268 && !HasRoleGroup(groupCns, facultyGroup, studentGroup))
            {
                _logger.LogInformation(
                    "LDAP: found {Sam} on Global Catalog; re-reading memberOf from port 389.",
                    foundSam);
                using var dc = CreateConnection(server, 389);
                var dcEntry = ReadByDn(dc, dn);
                if (dcEntry is not null)
                {
                    groupCns = ReadMemberOfCns(dcEntry);
                }
            }

            _logger.LogInformation(
                "LDAP: found {Dn} via {Server}:{Port} ({BaseDn}). sAMAccountName={Sam}, UPN={Upn}, mail={Mail}. Direct member of {GroupCount} groups. CNs: {GroupCns}",
                dn,
                usedServer,
                usedPort,
                usedBaseDn,
                foundSam,
                foundUpn,
                foundMail,
                groupCns.Count,
                string.Join(", ", groupCns));

            if (groupCns.Contains(facultyGroup, StringComparer.OrdinalIgnoreCase))
            {
                _logger.LogInformation(
                    "LDAP: {Sam} matched faculty group {Group}. Role: Faculty.",
                    foundSam,
                    facultyGroup);
                return UserRole.Faculty;
            }

            if (groupCns.Contains(studentGroup, StringComparer.OrdinalIgnoreCase))
            {
                _logger.LogInformation(
                    "LDAP: {Sam} matched student group {Group}. Role: Student.",
                    foundSam,
                    studentGroup);
                return UserRole.Student;
            }

            _logger.LogWarning(
                "LDAP: {Sam} is not in {FacultyGroup} or {StudentGroup}.",
                foundSam,
                facultyGroup,
                studentGroup);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "LDAP lookup failed for sAMAccountName={Sam}, UPN={Upn}, mail={Mail} against {Server}. Role will fall back to the database.",
                samAccountName,
                upn,
                mail,
                server);
            return null;
        }
    }

    private SearchResultEntry? FindUser(
        LdapConnection connection,
        string baseDn,
        string samAccountName,
        string upn,
        string mail)
    {
        var clauses = new List<string>();
        AddClause(clauses, "sAMAccountName", samAccountName);
        foreach (var candidate in ExpandUpnOrMail(upn, mail))
        {
            AddClause(clauses, "userPrincipalName", candidate);
            AddClause(clauses, "mail", candidate);
            AddClause(clauses, "proxyAddresses", $"SMTP:{candidate}");
            AddClause(clauses, "proxyAddresses", $"smtp:{candidate}");
        }

        if (clauses.Count == 0)
        {
            return null;
        }

        var filter = $"(&(objectCategory=person)(|{string.Join(string.Empty, clauses)}))";
        _logger.LogInformation("LDAP: filter {Filter}", filter);

        var request = new SearchRequest(
            baseDn,
            filter,
            SearchScope.Subtree,
            "sAMAccountName",
            "userPrincipalName",
            "mail",
            "memberOf",
            "distinguishedName");

        var response = (SearchResponse)connection.SendRequest(request);
        _logger.LogInformation(
            "LDAP: search result code {ResultCode}, entries {Count}.",
            response.ResultCode,
            response.Entries.Count);

        return response.Entries.Count == 0 ? null : response.Entries[0];
    }

    private static SearchResultEntry? ReadByDn(LdapConnection connection, string dn)
    {
        if (string.IsNullOrWhiteSpace(dn))
        {
            return null;
        }

        var request = new SearchRequest(dn, "(objectClass=*)", SearchScope.Base, "memberOf");
        var response = (SearchResponse)connection.SendRequest(request);
        return response.Entries.Count == 0 ? null : response.Entries[0];
    }

    private static bool HasRoleGroup(IReadOnlyCollection<string> groupCns, string facultyGroup, string studentGroup)
        => groupCns.Contains(facultyGroup, StringComparer.OrdinalIgnoreCase)
           || groupCns.Contains(studentGroup, StringComparer.OrdinalIgnoreCase);

    private IEnumerable<string> ExpandUpnOrMail(string upn, string mail)
    {
        var values = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!string.IsNullOrWhiteSpace(upn))
        {
            values.Add(upn);
        }

        if (!string.IsNullOrWhiteSpace(mail))
        {
            values.Add(mail);
        }

        var local = StripDomain(FirstNonEmpty(upn, mail));
        if (string.IsNullOrWhiteSpace(local))
        {
            return values;
        }

        var extraDomains = new[]
        {
            _configuration["AzureAd:Domain"],
            "aub.edu.lb",
            "mail.aub.edu",
            "mail.aub.edu.lb"
        };

        foreach (var domain in extraDomains)
        {
            if (!string.IsNullOrWhiteSpace(domain))
            {
                values.Add($"{local}@{domain.Trim()}");
            }
        }

        return values;
    }

    private static void AddClause(List<string> clauses, string attribute, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        clauses.Add($"({attribute}={EscapeFilter(value)})");
    }

    private static string? DiscoverBaseDn(LdapConnection connection)
    {
        var request = new SearchRequest(string.Empty, "(objectClass=*)", SearchScope.Base, "defaultNamingContext");
        var response = (SearchResponse)connection.SendRequest(request);
        if (response.Entries.Count == 0)
        {
            return null;
        }

        return Attr(response.Entries[0], "defaultNamingContext");
    }

    private LdapConnection CreateConnection(string server, int port)
    {
        var identifier = new LdapDirectoryIdentifier(server, port);
        var connection = new LdapConnection(identifier)
        {
            AuthType = AuthType.Negotiate
        };
        connection.SessionOptions.ProtocolVersion = 3;
        connection.SessionOptions.ReferralChasing = ReferralChasingOptions.All;
        connection.Timeout = TimeSpan.FromSeconds(20);

        var bindUser = _configuration["Ldap:BindUserName"];
        var bindPassword = _configuration["Ldap:BindPassword"];
        if (!string.IsNullOrWhiteSpace(bindUser) && !string.IsNullOrWhiteSpace(bindPassword))
        {
            connection.Credential = new NetworkCredential(bindUser, bindPassword);
        }

        connection.Bind();
        return connection;
    }

    private static List<string> ReadMemberOfCns(SearchResultEntry entry)
    {
        var cns = new List<string>();
        if (!entry.Attributes.Contains("memberOf"))
        {
            return cns;
        }

        foreach (var value in entry.Attributes["memberOf"].GetValues(typeof(string)))
        {
            var cn = ExtractCnFromDn((string)value);
            if (!string.IsNullOrEmpty(cn))
            {
                cns.Add(cn);
            }
        }

        return cns;
    }

    private static string Attr(SearchResultEntry entry, string attribute)
    {
        if (!entry.Attributes.Contains(attribute))
        {
            return string.Empty;
        }

        var values = entry.Attributes[attribute].GetValues(typeof(string));
        return values.Length > 0 ? (string)values[0] : string.Empty;
    }

    private static string FirstNonEmpty(string? first, string? second)
    {
        if (!string.IsNullOrWhiteSpace(first))
        {
            return first.Trim();
        }

        return string.IsNullOrWhiteSpace(second) ? string.Empty : second.Trim();
    }

    private static string StripDomain(string samAccountName)
    {
        var at = samAccountName.IndexOf('@');
        return (at >= 0 ? samAccountName[..at] : samAccountName).Trim();
    }

    /// <summary>
    /// "CN=ALLACADstaff-STF,OU=...,DC=aub,DC=edu,DC=lb" → "ALLACADstaff-STF"
    /// </summary>
    private static string? ExtractCnFromDn(string dn)
    {
        if (!dn.StartsWith("CN=", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var commaIndex = dn.IndexOf(',');
        return commaIndex > 3 ? dn[3..commaIndex] : dn[3..];
    }

    /// <summary>
    /// Hostnames like "win2k.aub.edu.lb" → "DC=aub,DC=edu,DC=lb".
    /// Plain domains like "aub.edu.lb" → "DC=aub,DC=edu,DC=lb".
    /// </summary>
    private static string BuildBaseDn(string domainOrServer)
    {
        var labels = domainOrServer.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (labels.Length >= 3)
        {
            labels = labels[1..];
        }

        return string.Join(",", labels.Select(p => $"DC={p}"));
    }

    private static string EscapeFilter(string value)
        => value
            .Replace("\\", "\\5c", StringComparison.Ordinal)
            .Replace("*", "\\2a", StringComparison.Ordinal)
            .Replace("(", "\\28", StringComparison.Ordinal)
            .Replace(")", "\\29", StringComparison.Ordinal)
            .Replace("\0", "\\00", StringComparison.Ordinal);
}
