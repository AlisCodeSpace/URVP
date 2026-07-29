// File: RICHConnect.Backend/DTOs/UserDto.cs

using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.DTOs.Users
{
    public class UserDto
    {
        // Changed from int ? Guid
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public string Email { get; set; } = null!;

        // Changed from string ? UserRole enum
        public UserRole Role { get; set; }

        // (Optional) If you later attach profile image URLs or CommunityPartner links,
        // you can populate these. But in the new schema, "CommunityPartnerOwner" is implied
        // by User ? Membership relationships, so you don't need a dedicated CommunityPartnerUrl here.
        public string? ProfileImageUrl { get; set; }
    }
}
