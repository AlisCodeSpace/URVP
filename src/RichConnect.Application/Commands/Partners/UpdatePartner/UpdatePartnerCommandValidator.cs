using FluentValidation;
using RICHConnect.Backend.Infrastructure.Data.Repositories.Partners.Interfaces;

namespace RICHConnect.Backend.Application.Commands.Partners.UpdatePartner
{
    /// <summary>
    /// Validator for UpdatePartnerCommand
    /// </summary>
    public class UpdatePartnerCommandValidator : AbstractValidator<UpdatePartnerCommand>
    {
        // SVG removed for security - SVG files can contain executable JavaScript when served inline
        private readonly string[] _allowedFileExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
        private const int _maxFileSizeInMb = 5;
        private const int _maxFileSizeInBytes = _maxFileSizeInMb * 1024 * 1024; // 5MB
        private readonly IPartnerRepository _partnerRepository;

        public UpdatePartnerCommandValidator(IPartnerRepository partnerRepository)
        {
            _partnerRepository = partnerRepository;

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            // Check if user has a partner profile
            RuleFor(x => x.UserId)
                .MustAsync(async (userId, cancellation) => 
                    await _partnerRepository.ExistsForUserAsync(userId))
                .WithMessage("User does not have a partner profile.");

            // Institution information
            RuleFor(x => x.InstitutionName)
                .MaximumLength(128).WithMessage("Institution name cannot exceed 128 characters.")
                .When(x => !string.IsNullOrEmpty(x.InstitutionName));

            RuleFor(x => x.InstitutionAddress)
                .MaximumLength(256).WithMessage("Institution address cannot exceed 256 characters.")
                .When(x => !string.IsNullOrEmpty(x.InstitutionAddress));

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(32).WithMessage("Phone number cannot exceed 32 characters.")
                .Matches(@"^[0-9+\-\s()]*$").WithMessage("Phone number contains invalid characters.")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

            RuleFor(x => x.RegistrationNumberArea)
                .MaximumLength(64).WithMessage("Registration number/area cannot exceed 64 characters.")
                .When(x => !string.IsNullOrEmpty(x.RegistrationNumberArea));

            RuleFor(x => x.Sector)
                .IsInEnum().WithMessage("Invalid sector value.")
                .When(x => x.Sector.HasValue);

            RuleFor(x => x.InstitutionSize)
                .IsInEnum().WithMessage("Invalid institution size value.")
                .When(x => x.InstitutionSize.HasValue);

            RuleFor(x => x.ChamberOfCommerceNumber)
                .MaximumLength(64).WithMessage("Chamber of commerce number cannot exceed 64 characters.")
                .When(x => !string.IsNullOrEmpty(x.ChamberOfCommerceNumber));

            RuleFor(x => x.Vision)
                .MaximumLength(2000).WithMessage("Vision cannot exceed 2000 characters.")
                .When(x => !string.IsNullOrEmpty(x.Vision));

            RuleFor(x => x.Mission)
                .MaximumLength(2000).WithMessage("Mission cannot exceed 2000 characters.")
                .When(x => !string.IsNullOrEmpty(x.Mission));

            RuleFor(x => x.CertificationNumber)
                .MaximumLength(64).WithMessage("Certification number cannot exceed 64 characters.")
                .When(x => !string.IsNullOrEmpty(x.CertificationNumber));

            RuleFor(x => x.AccreditationType)
                .IsInEnum().WithMessage("Invalid accreditation type value.")
                .When(x => x.AccreditationType.HasValue);

            // Logo validation
            RuleFor(x => x.Logo)
                .Must(logo => logo == null || logo.Length <= _maxFileSizeInBytes)
                .WithMessage($"Logo file size must not exceed {_maxFileSizeInMb}MB.")
                .When(x => x.Logo != null);

            RuleFor(x => x.Logo)
                .Must(logo => logo == null || _allowedFileExtensions.Contains(Path.GetExtension(logo.FileName).ToLowerInvariant()))
                .WithMessage($"Logo file must be one of the following types: {string.Join(", ", _allowedFileExtensions)}")
                .When(x => x.Logo != null);
        }
    }
}