using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RICHConnect.Backend.Api.Controllers.Base;
using System.ComponentModel.DataAnnotations;
using AUB.APIServices.FMIS.Contracts.Interfaces;
using AUB.APIServices.FMIS.Contracts.Classes;
using ProtoBuf.Grpc;
using Grpc.Core;

namespace RICHConnect.Backend.Api.Controllers.Faculty
{
    /// <summary>
    /// Controller for FMIS Faculty Members data
    /// Provides access to faculty member information from the FMIS system
    /// </summary>
    [ApiController]
    [Route("api/faculty/members")]
    [Authorize] // Require authentication for all endpoints
    public class FacultyMembersController : ApiControllerBase
    {
        private readonly ILogger<FacultyMembersController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        public FacultyMembersController(
            ILogger<FacultyMembersController> logger,
            IConfiguration configuration,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
        }

        private IFMISService? GetFmisService()
        {
            return _serviceProvider.GetService<IFMISService>();
        }

        /// <summary>
        /// Get all faculty members in a specific faculty
        /// </summary>
        /// <param name="faculty">Faculty code (e.g., "FAS", "MSFEA")</param>
        /// <returns>List of faculty members</returns>
        [HttpGet]
        public async Task<IActionResult> GetFacultyMembers([FromQuery] string faculty = "FAS")
        {
            // Validate faculty parameter to prevent injection attacks
            var allowedFaculties = _configuration.GetSection("Fmis:Faculties").Get<string[]>() ?? Array.Empty<string>();
            var isEnabled = IsFmisEnabled();

            // #region agent log
            AgentDebugLog(
                "pre-fix",
                "H2,H3,H5",
                "FacultyMembersController.cs:46",
                "GetFacultyMembers entered",
                new
                {
                    faculty,
                    allowedFacultiesCount = allowedFaculties.Length,
                    isEnabled,
                    serviceRegistered = GetFmisService() != null,
                    endpointHost = GetHost(_configuration["ServicesConfigurationEndPoint"])
                });
            // #endregion

            if (string.IsNullOrWhiteSpace(faculty) || faculty.Length > 20 || 
                !allowedFaculties.Contains(faculty, StringComparer.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Invalid faculty parameter provided: {Faculty}", faculty);
                return BadRequest(new
                {
                    Success = false,
                    Error = "Invalid faculty code.",
                    Message = $"Faculty must be one of: {string.Join(", ", allowedFaculties)}"
                });
            }

            // Check if FMIS is enabled
            if (!isEnabled)
            {
                // #region agent log
                AgentDebugLog(
                    "pre-fix",
                    "H2,H5",
                    "FacultyMembersController.cs:63",
                    "Returning 503 because FMIS is disabled",
                    new { faculty, allowedFacultiesCount = allowedFaculties.Length });
                // #endregion

                return StatusCode(503, new
                {
                    Success = false,
                    Error = "FMIS is not available.",
                    Message = "FMIS service is disabled or not configured. Please contact your administrator."
                });
            }

            var fmisService = GetFmisService();
            if (fmisService == null)
            {
                // #region agent log
                AgentDebugLog(
                    "pre-fix",
                    "H1,H3,H5",
                    "FacultyMembersController.cs:74",
                    "Returning 503 because IFMISService is not registered",
                    new
                    {
                        faculty,
                        isEnabled,
                        endpointHost = GetHost(_configuration["ServicesConfigurationEndPoint"])
                    });
                // #endregion

                return StatusCode(503, new
                {
                    Success = false,
                    Error = "FMIS service is not available.",
                    Message = "The FMIS service could not be initialized. This usually means the service endpoint is unreachable."
                });
            }

            try
            {
                var request = new GetFacultyMembersRequest { Faculty = faculty };
                var callContext = new CallContext(new Grpc.Core.CallOptions(cancellationToken: HttpContext.RequestAborted));

                // #region agent log
                AgentDebugLog(
                    "pre-fix",
                    "H4,H5",
                    "FacultyMembersController.cs:87",
                    "Calling FMIS GetFacultyMembersInFaculty",
                    new { faculty, isEnabled });
                // #endregion

                var facultyMembers = await fmisService.GetFacultyMembersInFaculty(request, callContext);
                
                return Ok(new
                {
                    Success = true,
                    Faculty = faculty,
                    Count = facultyMembers.Length,
                    FacultyMembers = facultyMembers.Select(fm => new
                    {
                        fm.MemberId,
                        fm.Firstname,
                        fm.Lastname,
                        fm.Email,
                        fm.Rank,
                        fm.Department,
                        fm.Faculty,
                        fm.FTE
                    })
                });
            }
            catch (Exception ex)
            {
                // #region agent log
                AgentDebugLog(
                    "pre-fix",
                    "H4",
                    "FacultyMembersController.cs:108",
                    "FMIS GetFacultyMembersInFaculty failed",
                    new
                    {
                        faculty,
                        exceptionType = ex.GetType().FullName,
                        ex.Message,
                        rpcStatusCode = ex is RpcException rpcException ? rpcException.StatusCode.ToString() : null,
                        innerExceptionType = ex.InnerException?.GetType().FullName,
                        innerMessage = ex.InnerException?.Message
                    });
                // #endregion

                _logger.LogError(ex, "Error fetching faculty members for faculty: {Faculty}", faculty);

                // Handle common authentication errors gracefully
                if (ex is RpcException rpcEx &&
                    rpcEx.InnerException is HttpRequestException httpEx &&
                    httpEx.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    return StatusCode(503, new
                    {
                        Success = false,
                        Faculty = faculty,
                        Error = "FMIS is not available (unauthorized).",
                        Message = "Authentication with FMIS service failed. Please contact your administrator."
                    });
                }

                return StatusCode(500, new
                {
                    Success = false,
                    Error = ex.Message,
                    Faculty = faculty
                });
            }
        }

        /// <summary>
        /// Get detailed information about a specific faculty member
        /// </summary>
        /// <param name="memberId">Faculty member ID</param>
        /// <returns>Detailed faculty member information</returns>
        [HttpGet("{memberId}")]
        public async Task<IActionResult> GetFacultyMember([FromRoute, StringLength(50, MinimumLength = 1)] string memberId)
        {
            // Validate memberId parameter to prevent injection attacks
            if (string.IsNullOrWhiteSpace(memberId) || memberId.Length > 50)
            {
                _logger.LogWarning("Invalid memberId parameter provided: {MemberId}", memberId);
                return BadRequest(new
                {
                    Success = false,
                    Error = "Invalid member ID.",
                    Message = "Member ID must be a non-empty string with maximum 50 characters."
                });
            }

            // Check if FMIS is enabled
            if (!IsFmisEnabled())
            {
                return StatusCode(503, new
                {
                    Success = false,
                    Error = "FMIS is not available.",
                    Message = "FMIS service is disabled or not configured. Please contact your administrator."
                });
            }

            var fmisService = GetFmisService();
            if (fmisService == null)
            {
                return StatusCode(503, new
                {
                    Success = false,
                    Error = "FMIS service is not available.",
                    Message = "The FMIS service could not be initialized. This usually means the service endpoint is unreachable."
                });
            }

            try
            {
                var request = new GetFacultyMemberRequest { MemberId = memberId };
                var callContext = new CallContext(new Grpc.Core.CallOptions(cancellationToken: HttpContext.RequestAborted));
                var facultyMember = await fmisService.GetFacultyMember(request, callContext);
                
                return Ok(new
                {
                    Success = true,
                    FacultyMember = facultyMember
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching faculty member with ID: {MemberId}", memberId);
                return StatusCode(500, new
                {
                    Success = false,
                    Error = ex.Message,
                    MemberId = memberId
                });
            }
        }
        
        /// <summary>
        /// Get detailed information about multiple faculty members
        /// </summary>
        /// <param name="memberIds">Array of faculty member IDs</param>
        /// <returns>List of detailed faculty member information</returns>
        [HttpGet("details")]
        public async Task<IActionResult> GetMultipleFacultyMembers([FromQuery, MaxLength(100)] string[] memberIds)
        {
            // Check if FMIS is enabled
            if (!IsFmisEnabled())
            {
                return StatusCode(503, new
                {
                    Success = false,
                    Error = "FMIS is not available.",
                    Message = "FMIS service is disabled or not configured. Please contact your administrator."
                });
            }

            var fmisService = GetFmisService();
            if (fmisService == null)
            {
                return StatusCode(503, new
                {
                    Success = false,
                    Error = "FMIS service is not available.",
                    Message = "The FMIS service could not be initialized. This usually means the service endpoint is unreachable."
                });
            }

            try
            {
                if (memberIds == null || memberIds.Length == 0)
                {
                    return BadRequest(new { Success = false, Error = "No member IDs provided" });
                }

                // Validate memberIds array size to prevent DoS
                const int maxMemberIds = 100;
                if (memberIds.Length > maxMemberIds)
                {
                    _logger.LogWarning("Too many member IDs provided: {Count}", memberIds.Length);
                    return BadRequest(new 
                    { 
                        Success = false, 
                        Error = $"Maximum {maxMemberIds} member IDs allowed per request." 
                    });
                }
                
                var facultyMembers = new List<FacultyMember>();
                
                foreach (var memberId in memberIds)
                {
                    // Validate each memberId to prevent injection attacks
                    if (string.IsNullOrWhiteSpace(memberId) || memberId.Length > 50)
                    {
                        _logger.LogWarning("Invalid memberId in array: {MemberId}", memberId);
                        continue; // Skip invalid IDs instead of failing entire request
                    }
                    
                    try
                    {
                        var request = new GetFacultyMemberRequest { MemberId = memberId };
                        var callContext = new CallContext(new Grpc.Core.CallOptions(cancellationToken: HttpContext.RequestAborted));
                        var facultyMember = await fmisService.GetFacultyMember(request, callContext);
                        facultyMembers.Add(facultyMember);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error fetching faculty member with ID: {MemberId}", memberId);
                    }
                }
                
                return Ok(new
                {
                    Success = true,
                    Count = facultyMembers.Count,
                    FacultyMembers = facultyMembers
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching multiple faculty members");
                return StatusCode(500, new
                {
                    Success = false,
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Get FMIS service status
        /// </summary>
        /// <returns>FMIS service availability status</returns>
        [HttpGet("status")]
        public async Task<IActionResult> GetFmisStatus()
        {
            var isEnabled = IsFmisEnabled();
            var configurationEndpoint = _configuration["ServicesConfigurationEndPoint"];
            var serviceAvailable = GetFmisService() != null;
            var faculties = _configuration.GetSection("Fmis:Faculties").Get<string[]>() ?? Array.Empty<string>();
            var startupState = AppContext.GetData("RichConnect.FmisStartup.State")?.ToString();
            var startupExceptionMessage = AppContext.GetData("RichConnect.FmisStartup.ExceptionMessage")?.ToString();
            var configEndpointProbe = await ProbeConfigurationEndpointAsync(configurationEndpoint, HttpContext.RequestAborted);

            // #region agent log
            AgentDebugLog(
                "pre-fix",
                "H2,H3,H5",
                "FacultyMembersController.cs:297",
                "GetFmisStatus snapshot",
                new
                {
                    isEnabled,
                    serviceAvailable,
                    facultiesCount = faculties.Length,
                    endpointHost = GetHost(configurationEndpoint),
                    startupState,
                    configEndpointReachable = configEndpointProbe.Reachable,
                    configEndpointError = configEndpointProbe.Error
                });
            // #endregion
            
            return Ok(new
            {
                Success = true,
                FmisEnabled = isEnabled,
                FmisServiceAvailable = serviceAvailable,
                ConfigurationEndpoint = configurationEndpoint,
                ConfigurationEndpointProbe = configEndpointProbe,
                DebugSessionId = "e90e78",
                FacultiesCount = faculties.Length,
                StartupDiagnostics = new
                {
                    State = AppContext.GetData("RichConnect.FmisStartup.State"),
                    RegistrationState = AppContext.GetData("RichConnect.FmisStartup.RegistrationState"),
                    EndpointHost = AppContext.GetData("RichConnect.FmisStartup.EndpointHost"),
                    HasConfig = AppContext.GetData("RichConnect.FmisStartup.HasConfig"),
                    HasFmisEndpoint = AppContext.GetData("RichConnect.FmisStartup.HasFmisEndpoint"),
                    ExceptionType = AppContext.GetData("RichConnect.FmisStartup.ExceptionType"),
                    ExceptionMessage = startupExceptionMessage
                },
                RootCause = !serviceAvailable && startupState == "AddApiServicesFailed"
                    ? "The backend could not fetch the AUB services configuration endpoint during startup, so the FMIS gRPC client was never registered."
                    : null,
                RecommendedAction = !serviceAvailable && startupState == "AddApiServicesFailed"
                    ? "Verify production server network/firewall/DNS/TLS access to ServicesConfigurationEndPoint from the app host."
                    : null,
                Message = serviceAvailable && isEnabled
                    ? "FMIS is enabled and available" 
                    : serviceAvailable && !isEnabled
                    ? "FMIS service is available but disabled in configuration."
                    : !serviceAvailable && isEnabled
                    ? "FMIS is enabled but service is not available. Ensure the service endpoint is reachable."
                    : "FMIS is disabled or not configured."
            });
        }

        /// <summary>
        /// Checks if FMIS is enabled via configuration
        /// Uses TestingEnabled for backward compatibility, but applies to production use
        /// </summary>
        private bool IsFmisEnabled()
        {
            return _configuration.GetValue<bool>("Fmis:TestingEnabled", false);
        }

        private sealed class EndpointProbeResult
        {
            public bool Reachable { get; init; }
            public string? Error { get; init; }
            public object Details { get; init; } = new { };
        }

        private static async Task<EndpointProbeResult> ProbeConfigurationEndpointAsync(string? endpoint, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                return new EndpointProbeResult
                {
                    Reachable = false,
                    Error = "ServicesConfigurationEndPoint is not configured.",
                    Details = new
                    {
                        Reachable = false,
                        Error = "ServicesConfigurationEndPoint is not configured."
                    }
                };
            }

            var startedAt = DateTimeOffset.UtcNow;
            try
            {
                using var handler = new HttpClientHandler();
                using var client = new HttpClient(handler)
                {
                    Timeout = TimeSpan.FromSeconds(5)
                };

                using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                using var response = await client.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    ct);

                return new EndpointProbeResult
                {
                    Reachable = true,
                    Details = new
                    {
                        Reachable = true,
                        StatusCode = (int)response.StatusCode,
                        response.ReasonPhrase,
                        ElapsedMs = (int)(DateTimeOffset.UtcNow - startedAt).TotalMilliseconds
                    }
                };
            }
            catch (Exception ex)
            {
                return new EndpointProbeResult
                {
                    Reachable = false,
                    Error = ex.Message,
                    Details = new
                    {
                        Reachable = false,
                        ExceptionType = ex.GetType().FullName,
                        Error = ex.Message,
                        InnerExceptionType = ex.InnerException?.GetType().FullName,
                        InnerError = ex.InnerException?.Message,
                        ElapsedMs = (int)(DateTimeOffset.UtcNow - startedAt).TotalMilliseconds
                    }
                };
            }
        }

        private static void AgentDebugLog(string runId, string hypothesisId, string location, string message, object data)
        {
            try
            {
                var payload = new
                {
                    sessionId = "e90e78",
                    runId,
                    hypothesisId,
                    location,
                    message,
                    data,
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };
                System.IO.File.AppendAllText(GetAgentDebugLogPath(), System.Text.Json.JsonSerializer.Serialize(payload) + Environment.NewLine);
            }
            catch
            {
                // Debug instrumentation must never affect request handling.
            }
        }

        private static string? GetHost(string? url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uri) ? uri.Host : null;
        }

        private static string GetAgentDebugLogPath()
        {
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, ".git")))
            {
                directory = directory.Parent;
            }

            return Path.Combine(directory?.FullName ?? Directory.GetCurrentDirectory(), "debug-e90e78.log");
        }
    }
}
