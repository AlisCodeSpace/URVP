using FluentValidation;
using RICHConnect.Backend.Application.DTOs.Partners;

namespace RICHConnect.Backend.Application.Validators.Partners
{
    /// <summary>
    /// Validator for CommunityPartner profile update requests
    /// </summary>
    public class UpdateCommunityPartnerDtoValidator : AbstractValidator<UpdateCommunityPartnerDto>
    {
        // SVG removed for security - SVG files can contain executable JavaScript when served inline
        private readonly string[] _allowedFileExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
        private const int _maxFileSizeInMb = 5;
        private const int _maxFileSizeInBytes = _maxFileSizeInMb * 1024 * 1024; // 5MB

        public UpdateCommunityPartnerDtoValidator()
        {
            // Institution information
            RuleFor(x => x.InstitutionName)
                .MaximumLength(128).WithMessage("Institution name cannot exceed 128 characters.");

            RuleFor(x => x.InstitutionAddress)
                .MaximumLength(256).WithMessage("Institution address cannot exceed 256 characters.");

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(32).WithMessage("Phone number cannot exceed 32 characters.")
                .Matches(@"^[0-9+\-\s()]*$").WithMessage("Phone number contains invalid characters.");

            RuleFor(x => x.RegistrationNumberArea)
                .MaximumLength(64).WithMessage("Registration number/area cannot exceed 64 characters.");

            RuleFor(x => x.ChamberOfCommerceNumber)
                .MaximumLength(64).WithMessage("Chamber of commerce number cannot exceed 64 characters.");

            RuleFor(x => x.Vision)
                .MaximumLength(2000).WithMessage("Vision statement cannot exceed 2000 characters.");

            RuleFor(x => x.Mission)
                .MaximumLength(2000).WithMessage("Mission statement cannot exceed 2000 characters.");

            RuleFor(x => x.CertificationNumber)
                .MaximumLength(64).WithMessage("Certification number cannot exceed 64 characters.");

            // Logo validation
            RuleFor(x => x.Logo)
                .Must(logo => logo == null || logo.Length <= _maxFileSizeInBytes)
                .WithMessage($"Logo file size must not exceed {_maxFileSizeInMb}MB.")
                .Must(logo => logo == null || _allowedFileExtensions.Contains(Path.GetExtension(logo.FileName).ToLowerInvariant()))
                .WithMessage($"Logo file must be one of the following types: {string.Join(", ", _allowedFileExtensions)}");
        }
    }
} 