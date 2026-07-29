using RICHConnect.Backend.Domain.Enums;
namespace RICHConnect.Backend.Domain.Events
{
    /// <summary>
    /// Event raised when a user logs in to the system
    /// </summary>
    public class UserLoggedInEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "UserLoggedIn";
        
        /// <summary>
        /// ID of the user who logged in
        /// </summary>
        public Guid UserId { get; }
        
        /// <summary>
        /// Email address of the user
        /// </summary>
        public string Email { get; }
        
        /// <summary>
        /// Display name of the user
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Role of the user
        /// </summary>
        public UserRole Role { get; }
        
        /// <summary>
        /// Authentication provider used (e.g., "AzureAd", "AzureB2C")
        /// </summary>
        public string AuthenticationProvider { get; }
        
        /// <summary>
        /// Timestamp when the login occurred
        /// </summary>
        public DateTime LoginTimestamp { get; }
        
        /// <summary>
        /// Optional correlation ID for tracking authentication flow
        /// </summary>
        public string? CorrelationId { get; }

        public UserLoggedInEvent(
            Guid userId,
            string email,
            string name,
            UserRole role,
            string authenticationProvider,
            string? correlationId = null)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            UserId = userId;
            Email = email;
            Name = name;
            Role = role;
            AuthenticationProvider = authenticationProvider;
            LoginTimestamp = DateTime.UtcNow;
            CorrelationId = correlationId;
        }
    }

    /// <summary>
    /// Event raised when a new user is registered in the system
    /// </summary>
    public class UserRegisteredEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "UserRegistered";
        
        /// <summary>
        /// ID of the newly registered user
        /// </summary>
        public Guid UserId { get; }
        
        /// <summary>
        /// Email address of the user
        /// </summary>
        public string Email { get; }
        
        /// <summary>
        /// Display name of the user
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// Role assigned to the user
        /// </summary>
        public UserRole Role { get; }
        
        /// <summary>
        /// Authentication provider used for registration (e.g., "AzureAd", "AzureB2C")
        /// </summary>
        public string AuthenticationProvider { get; }
        
        /// <summary>
        /// URL to the user's profile image (if available)
        /// </summary>
        public string? ProfileImageUrl { get; }
        
        /// <summary>
        /// Optional correlation ID for tracking registration flow
        /// </summary>
        public string? CorrelationId { get; }

        public UserRegisteredEvent(
            Guid userId,
            string email,
            string name,
            UserRole role,
            string authenticationProvider,
            string? profileImageUrl = null,
            string? correlationId = null)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            UserId = userId;
            Email = email;
            Name = name;
            Role = role;
            AuthenticationProvider = authenticationProvider;
            ProfileImageUrl = profileImageUrl;
            CorrelationId = correlationId;
        }
    }

    /// <summary>
    /// Event raised when a user is authenticated (either during login or token validation)
    /// </summary>
    public class UserAuthenticatedEvent : IDomainEvent
    {
        public Guid EventId { get; }
        public DateTime OccurredAt { get; }
        public string EventType => "UserAuthenticated";
        
        /// <summary>
        /// ID of the authenticated user
        /// </summary>
        public Guid UserId { get; }
        
        /// <summary>
        /// Email address of the user
        /// </summary>
        public string Email { get; }
        
        /// <summary>
        /// Authentication provider used (e.g., "AzureAd", "AzureB2C")
        /// </summary>
        public string AuthenticationProvider { get; }
        
        /// <summary>
        /// Authentication method used (e.g., "OIDC", "Cookie")
        /// </summary>
        public string AuthenticationMethod { get; }
        
        /// <summary>
        /// Whether this was a new session or a token refresh
        /// </summary>
        public bool IsNewSession { get; }
        
        /// <summary>
        /// Optional correlation ID for tracking authentication flow
        /// </summary>
        public string? CorrelationId { get; }

        public UserAuthenticatedEvent(
            Guid userId,
            string email,
            string authenticationProvider,
            string authenticationMethod,
            bool isNewSession,
            string? correlationId = null)
        {
            EventId = Guid.NewGuid();
            OccurredAt = DateTime.UtcNow;
            UserId = userId;
            Email = email;
            AuthenticationProvider = authenticationProvider;
            AuthenticationMethod = authenticationMethod;
            IsNewSession = isNewSession;
            CorrelationId = correlationId;
        }
    }
}
