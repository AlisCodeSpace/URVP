using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RICHConnect.Backend.Api.Controllers.Base;
using RICHConnect.Backend.Application.Commands.Settings.DeleteSetting;
using RICHConnect.Backend.Application.Commands.Settings.SetSetting;
using RICHConnect.Backend.Application.DTOs.Settings;
using RICHConnect.Backend.Application.Queries.Settings.GetSettingByKey;
using RICHConnect.Backend.Application.Queries.Settings.ListSettings;
using System.ComponentModel.DataAnnotations;

namespace RICHConnect.Backend.Api.Controllers.Settings
{
    /// <summary>
    /// Admin-only API for managing application settings (configuration and secrets). Values are masked for secrets unless explicitly revealed.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class SettingsController : ApiControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SettingsController> _logger;

        public SettingsController(IMediator mediator, ILogger<SettingsController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// GET: api/Settings — List settings. Optional category filter and includeSecretValues (default false).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> List([FromQuery] string? category = null, [FromQuery] bool includeSecretValues = false)
        {
            var query = new ListSettingsQuery
            {
                Category = category,
                IncludeSecretValues = includeSecretValues
            };
            var result = await _mediator.Send(query);
            return SuccessResponse(result);
        }

        /// <summary>
        /// GET: api/Settings/{key} — Get a single setting by key. Use ?reveal=true to reveal secret value.
        /// </summary>
        [HttpGet("{key}")]
        public async Task<IActionResult> GetByKey([FromRoute, StringLength(100, MinimumLength = 1)] string key, [FromQuery] bool reveal = false)
        {
            var query = new GetSettingByKeyQuery
            {
                Key = key,
                RevealSecret = reveal
            };
            var result = await _mediator.Send(query);
            if (result == null)
                return ResourceNotFound("Setting", key);
            return SuccessResponse(result);
        }

        /// <summary>
        /// PUT: api/Settings/{key} — Create or update a setting. Response value is masked when secret.
        /// </summary>
        [HttpPut("{key}")]
        public async Task<IActionResult> Put([FromRoute, StringLength(100, MinimumLength = 1)] string key, [FromBody] SetSettingRequestDto body)
        {
            if (body == null)
                return BadRequest(new { message = "Request body is required." });

            var updatedBy = GetCurrentUserId();
            var command = new SetSettingCommand
            {
                Key = key,
                Value = body.Value,
                IsSecret = body.IsSecret,
                Category = body.Category,
                Description = body.Description,
                UpdatedBy = updatedBy
            };

            try
            {
                var result = await _mediator.Send(command);
                return SuccessResponse(result);
            }
            catch (FluentValidation.ValidationException)
            {
                return ValidationProblem();
            }
        }

        /// <summary>
        /// DELETE: api/Settings/{key} — Delete a setting. Returns 204 on success, 404 if not found.
        /// </summary>
        [HttpDelete("{key}")]
        public async Task<IActionResult> Delete([FromRoute, StringLength(100, MinimumLength = 1)] string key)
        {
            var adminUserId = GetCurrentUserId();
            var command = new DeleteSettingCommand
            {
                Key = key,
                AdminUserId = adminUserId
            };
            var deleted = await _mediator.Send(command);
            if (!deleted)
                return ResourceNotFound("Setting", key);
            return NoContent();
        }
    }
}
