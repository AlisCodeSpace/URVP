using RICHConnect.Backend.Application.Handlers;
using RICHConnect.Backend.Application.Handlers.Auth;
using RICHConnect.Backend.Application.Handlers.Challenges;
using RICHConnect.Backend.Application.Handlers.Notifications;
using RICHConnect.Backend.Application.Handlers.Partners;
using RICHConnect.Backend.Application.Handlers.ResearchFields;
using RICHConnect.Backend.Application.Handlers.Themes;
using RICHConnect.Backend.Application.Handlers.RDProjects;
using RICHConnect.Backend.Domain.Events;
using RICHConnect.Backend.Infrastructure.Events;

namespace RICHConnect.Backend.Api.Configuration.DependencyInjection
{
    /// <summary>
    /// Configuration for event infrastructure
    /// </summary>
    public static class EventInfrastructureConfiguration
    {
        /// <summary>
        /// Configure event infrastructure (event bus and handlers)
        /// </summary>
        public static IServiceCollection AddEventInfrastructure(this IServiceCollection services)
        {
            // Register Event Bus
            services.AddScoped<IEventBus, InMemoryEventBus>();
            
            // Challenge Event Handlers
            services.AddScoped<IEventHandler<ChallengeSubmittedEvent>, ChallengeSubmittedEventHandler>();
            services.AddScoped<IEventHandler<ChallengeApprovedEvent>, ChallengeApprovedEventHandler>();
            services.AddScoped<IEventHandler<ChallengeRejectedEvent>, ChallengeRejectedEventHandler>();
            services.AddScoped<IEventHandler<ChallengeMatchedEvent>, ChallengeMatchedEventHandler>();
            services.AddScoped<IEventHandler<FacultySpecialistInvitedEvent>, FacultySpecialistInvitedEventHandler>();
            services.AddScoped<IEventHandler<FacultySpecialistRespondedEvent>, FacultySpecialistRespondedEventHandler>();
            services.AddScoped<IEventHandler<ChallengeStatusChangedEvent>, ChallengeStatusChangedEventHandler>();
            services.AddScoped<IEventHandler<ChallengeUpdatedEvent>, ChallengeUpdatedEventHandler>();
            services.AddScoped<IEventHandler<ChallengeEditRequestedEvent>, ChallengeEditRequestedEventHandler>();
            services.AddScoped<IEventHandler<ChallengeEditRequestApprovedEvent>, ChallengeEditRequestApprovedEventHandler>();
            services.AddScoped<IEventHandler<ChallengeEditRequestRejectedEvent>, ChallengeEditRequestRejectedEventHandler>();
            
            // Partner Event Handlers
            services.AddScoped<IEventHandler<PartnerRegisteredEvent>, PartnerRegisteredEventHandler>();
            services.AddScoped<IEventHandler<PartnerApprovedEvent>, PartnerApprovedEventHandler>();
            services.AddScoped<IEventHandler<PartnerRejectedEvent>, PartnerRejectedEventHandler>();
            services.AddScoped<IEventHandler<PartnerUpdatedEvent>, PartnerUpdatedEventHandler>();
            
            // Auth Event Handlers
            services.AddScoped<IEventHandler<UserRegisteredEvent>, UserRegisteredEventHandler>();
            services.AddScoped<IEventHandler<UserLoggedInEvent>, UserLoggedInEventHandler>();
            services.AddScoped<IEventHandler<UserAuthenticatedEvent>, UserAuthenticatedEventHandler>();
            
            // ResearchField Event Handlers
            services.AddScoped<IEventHandler<ResearchFieldCreatedEvent>, ResearchFieldCreatedEventHandler>();
            services.AddScoped<IEventHandler<ResearchFieldApprovedEvent>, ResearchFieldApprovedEventHandler>();
            services.AddScoped<IEventHandler<ResearchFieldRejectedEvent>, ResearchFieldRejectedEventHandler>();
            services.AddScoped<IEventHandler<ResearchFieldUpdatedEvent>, ResearchFieldUpdatedEventHandler>();
            services.AddScoped<IEventHandler<ResearchFieldDeletedEvent>, ResearchFieldDeletedEventHandler>();
            
            // Theme Event Handlers
            services.AddScoped<IEventHandler<ThemeSubmittedEvent>, ThemeSubmittedEventHandler>();
            services.AddScoped<IEventHandler<ThemeApprovedEvent>, ThemeApprovedEventHandler>();
            services.AddScoped<IEventHandler<ThemeRejectedEvent>, ThemeRejectedEventHandler>();
            services.AddScoped<IEventHandler<ThemeUpdatedEvent>, ThemeUpdatedEventHandler>();
            services.AddScoped<IEventHandler<ThemeDeletedEvent>, ThemeDeletedEventHandler>();
            
            // RDProject Event Handlers
            services.AddScoped<IEventHandler<RDProjectSubmittedEvent>, RDProjectSubmittedEventHandler>();
            services.AddScoped<IEventHandler<RDProjectApprovedEvent>, RDProjectApprovedEventHandler>();
            services.AddScoped<IEventHandler<RDProjectRejectedEvent>, RDProjectRejectedEventHandler>();
            services.AddScoped<IEventHandler<RDProjectMatchedEvent>, RDProjectMatchedEventHandler>();
            services.AddScoped<IEventHandler<RDProjectFacultySpecialistInvitedEvent>, RDProjectFacultySpecialistInvitedEventHandler>();
            services.AddScoped<IEventHandler<RDProjectFacultySpecialistRespondedEvent>, RDProjectFacultySpecialistRespondedEventHandler>();
            services.AddScoped<IEventHandler<RDProjectEditRequestedEvent>, RDProjectEditRequestedEventHandler>();
            services.AddScoped<IEventHandler<RDProjectEditRequestApprovedEvent>, RDProjectEditRequestApprovedEventHandler>();
            services.AddScoped<IEventHandler<RDProjectEditRequestRejectedEvent>, RDProjectEditRequestRejectedEventHandler>();
            
            // Notification Event Handlers
            services.AddScoped<IEventHandler<NotificationCreatedEvent>, NotificationCreatedEventHandler>();
            services.AddScoped<IEventHandler<NotificationReadEvent>, NotificationReadEventHandler>();
            services.AddScoped<IEventHandler<NotificationDeletedEvent>, NotificationDeletedEventHandler>();
            services.AddScoped<IEventHandler<NotificationSettingsUpdatedEvent>, NotificationSettingsUpdatedEventHandler>();
            
            return services;
        }
    }
}
