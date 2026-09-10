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
            LdapConnection? foundOn = null;
            string? usedServer = null;
            var usedPort = 0;
            string? usedBaseDn = null;

            foreach (var port in ports.Distinct())
            {
                LdapConnection? connection = CreateConnection(server, port);
                try
                {
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
                        foundOn = connection;
                        connection = null;
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
                finally
                {
                    connection?.Dispose();
                }
            }

            if (entry is null || foundOn is null || usedBaseDn is null)
            {
                _logger.LogWarning(
                    "LDAP: AD did not return a user object. Entra can still show {Upn} because cloud identity is not the same as an LDAP search. Role will fall back to the mailbox domain.",
                    upn);
                return null;
            }

            using (foundOn)
            {
                var membershipConnection = foundOn;
                LdapConnection? dc = null;
                SearchResultEntry membershipEntry = entry;

                // Global Catalog (3268) omits non-universal groups from memberOf and from
                // nested-member checks. Always re-read membership from a DC (389).
                if (usedPort != 389)
                {
                    try
                    {
                        dc = CreateConnection(server, 389);
                        var dcEntry = ReadByDn(dc, entry.DistinguishedName);
                        if (dcEntry is not null)
                        {
                            membershipConnection = dc;
                            membershipEntry = dcEntry;
                            usedPort = 389;
                            _logger.LogInformation(
                                "LDAP: re-reading group membership for {Sam} from port 389.",
                                Attr(membershipEntry, "sAMAccountName"));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(
                            ex,
                            "LDAP: could not re-read membership on port 389; using {Port}.",
                            usedPort);
                    }
                }

                using (dc)
                {
                    var foundSam = Attr(membershipEntry, "sAMAccountName");
                    var foundUpn = Attr(membershipEntry, "userPrincipalName");
                    var foundMail = Attr(membershipEntry, "mail");
                    var dn = membershipEntry.DistinguishedName;
                    var groupCns = ReadMemberOfCns(membershipEntry);

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

                    var role = ResolveMembership(
                        membershipConnection,
                        usedBaseDn,
                        dn,
                        groupCns,
                        facultyGroup,
                        studentGroup);

                    if (role == UserRole.Faculty)
                    {
                        _logger.LogInformation(
                            "LDAP: {Sam} matched faculty group {Group}. Role: Faculty.",
                            foundSam,
                            facultyGroup);
                        return UserRole.Faculty;
                    }

                    if (role == UserRole.Student)
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
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "LDAP lookup failed for sAMAccountName={Sam}, UPN={Upn}, mail={Mail} against {Server}. Role will fall back to the mailbox domain.",
                samAccountName,
                upn,
                mail,
                server);
            return null;
        }
    }

    private UserRole? ResolveMembership(
        LdapConnection connection,
        string baseDn,
        string userDn,
        IReadOnlyCollection<string> directGroupCns,
        string facultyGroup,
        string studentGroup)
    {
        var facultyDn = FindGroupDn(connection, baseDn, facultyGroup);
        var studentDn = FindGroupDn(connection, baseDn, studentGroup);

        var inFaculty = facultyDn is not null && IsTransitiveMember(connection, userDn, facultyDn);
        var inStudent = studentDn is not null && IsTransitiveMember(connection, userDn, studentDn);

        if (inFaculty || inStudent)
        {
            return LdapDirectoryQuery.RoleFromMembership(inFaculty, inStudent);
        }

        return LdapDirectoryQuery.RoleFromGroupCns(directGroupCns, facultyGroup, studentGroup);
    }

    private string? FindGroupDn(LdapConnection connection, string baseDn, string cn)
    {
        try
        {
            var request = new SearchRequest(
                baseDn,
                LdapDirectoryQuery.GroupByCnFilter(cn),
                SearchScope.Subtree,
                "distinguishedName",
                "cn")
            {
                SizeLimit = 5
            };

            var response = (SearchResponse)connection.SendRequest(request);
            foreach (SearchResultEntry entry in response.Entries)
            {
                var foundCn = Attr(entry, "cn");
                if (foundCn.Equals(cn, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(
                        LdapDirectoryQuery.ExtractCnFromDn(entry.DistinguishedName),
                        cn,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return entry.DistinguishedName;
                }
            }

            _logger.LogWarning("LDAP: group CN {Group} was not found under {BaseDn}.", cn, baseDn);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "LDAP: failed to resolve group CN {Group}.", cn);
            return null;
        }
    }

    private bool IsTransitiveMember(LdapConnection connection, string userDn, string groupDn)
    {
        try
        {
            var request = new SearchRequest(
                groupDn,
                LdapDirectoryQuery.NestedMemberFilter(userDn),
                SearchScope.Base,
                "distinguishedName")
            {
                SizeLimit = 1
            };

            var response = (SearchResponse)connection.SendRequest(request);
            return response.Entries.Count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "LDAP: nested member check failed for {UserDn} in {GroupDn}.",
                userDn,
                groupDn);
            return false;
        }
    }

    private SearchResultEntry? FindUser(
        LdapConnection connection,
        string baseDn,
        string samAccountName,
        string upn,
        string mail)
    {
        var extra = ExpandUpnOrMail(upn, mail)
            .Where(address =>
                !address.Equals(upn, StringComparison.OrdinalIgnoreCase)
                && !address.Equals(mail, StringComparison.OrdinalIgnoreCase));

        var sequence = LdapDirectoryQuery.BuildUserLookupSequence(samAccountName, upn, mail, extra);

        foreach (var (attribute, value) in sequence)
        {
            var filter = LdapDirectoryQuery.UserObjectFilter(attribute, value);
            _logger.LogInformation("LDAP: filter {Filter}", filter);

            var request = new SearchRequest(
                baseDn,
                filter,
                SearchScope.Subtree,
                "sAMAccountName",
                "userPrincipalName",
                "mail",
                "memberOf",
                "distinguishedName")
            {
                SizeLimit = 5
            };

            SearchResponse response;
            try
            {
                response = (SearchResponse)connection.SendRequest(request);
            }
            catch (DirectoryOperationException ex) when (ex.Response is SearchResponse partial)
            {
                response = partial;
            }

            _logger.LogInformation(
                "LDAP: search result code {ResultCode}, entries {Count}.",
                response.ResultCode,
                response.Entries.Count);

            if (response.Entries.Count == 0)
            {
                continue;
            }

            if (response.Entries.Count == 1)
            {
                return response.Entries[0];
            }

            foreach (SearchResultEntry candidate in response.Entries)
            {
                if (Attr(candidate, "sAMAccountName")
                    .Equals(samAccountName, StringComparison.OrdinalIgnoreCase))
                {
                    return candidate;
                }
            }

            _logger.LogWarning(
                "LDAP: filter matched {Count} users; skipping ambiguous result for {Attribute}={Value}.",
                response.Entries.Count,
                attribute,
                value);
        }

        return null;
    }

    private static SearchResultEntry? ReadByDn(LdapConnection connection, string dn)
    {
        if (string.IsNullOrWhiteSpace(dn))
        {
            return null;
        }

        var request = new SearchRequest(
            dn,
            "(objectClass=*)",
            SearchScope.Base,
            "sAMAccountName",
            "userPrincipalName",
            "mail",
            "memberOf",
            "distinguishedName");
        var response = (SearchResponse)connection.SendRequest(request);
        return response.Entries.Count == 0 ? null : response.Entries[0];
    }

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

    private LdapConnection CreateConnection(string server, int port)
    {
        var identifier = new LdapDirectoryIdentifier(server, port);
        var connection = new LdapConnection(identifier)
        {
            AuthType = AuthType.Negotiate
        };
        connection.SessionOptions.ProtocolVersion = 3;
        connection.SessionOptions.ReferralChasing = ReferralChasingOptions.None;
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

    private static List<string> ReadMemberOfCns(SearchResultEntry entry)
    {
        var cns = new List<string>();
        if (!entry.Attributes.Contains("memberOf"))
        {
            return cns;
        }

        foreach (var value in entry.Attributes["memberOf"].GetValues(typeof(string)))
        {
            var cn = LdapDirectoryQuery.ExtractCnFromDn((string)value);
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
}
