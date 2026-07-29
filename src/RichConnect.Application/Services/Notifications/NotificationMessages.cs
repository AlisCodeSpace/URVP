namespace RICHConnect.Backend.Application.Services.Notifications
{
    /// <summary>
    /// Centralized notification message templates to ensure consistency
    /// across all notification types (in-app and email)
    /// </summary>
    public static class NotificationMessages
    {
        private static string ResolveFacultySpecialistDisplayName(string? facultySpecialistName) =>
            string.IsNullOrWhiteSpace(facultySpecialistName) ? "the faculty member" : facultySpecialistName.Trim();

        // ================================================================
        // CHALLENGE NOTIFICATIONS
        // ================================================================

        public static class Challenge
        {
            public static string SubmittedTitle() => "New Challenge Submitted";
            public static string SubmittedMessage(string challengeTitle) =>
                $"A new challenge '{challengeTitle}' has been submitted and is being reviewed by the team.";

            public static string ApprovedTitle() => "Challenge Ready for Matching";
            public static string ApprovedMessage(string challengeTitle) =>
                $"Great news! Your challenge '{challengeTitle}' has been reviewed and is now ready for faculty specialist matching. You'll be notified when faculty specialists show interest.";

            public static string RejectedTitle() => "Challenge Needs Updates";
            public static string RejectedMessage(string challengeTitle, string reason) =>
                $"Thank you for submitting your challenge '{challengeTitle}'. To move forward, please review the following feedback and update your challenge: {reason}";

            public static string MatchedTitle() => "Challenge Matched with Professors";
            public static string MatchedMessageAdmin(string challengeTitle, int count, string FacultySpecialistNames) =>
                $"Challenge '{challengeTitle}' has been matched with {count} facultySpecialist(s): {FacultySpecialistNames}.";
            public static string MatchedMessagePartner(string challengeTitle, int count, string FacultySpecialistNames) =>
                $"Exciting news! Your challenge '{challengeTitle}' has been successfully matched with {count} facultySpecialist(s): {FacultySpecialistNames}. You can now collaborate on this challenge.";

            public static string EditRequestedTitle() => "Challenge Edit Request Submitted";
            public static string EditRequestedMessage(string challengeTitle, string reason) =>
                $"A Community Partner has requested changes to their challenge '{challengeTitle}'. " +
                $"Please review and apply the requested changes. Reason: {reason}";

            public static string EditRequestApprovedTitle() => "Challenge Updates Applied";
            public static string EditRequestApprovedMessage(string? adminResponse) =>
                $"Your requested changes to the challenge have been processed and applied." +
                (!string.IsNullOrEmpty(adminResponse) ? $" Team note: {adminResponse}" : "");

            public static string EditRequestRejectedTitle() => "Challenge Edit Request - Additional Information Needed";
            public static string EditRequestRejectedMessage(string reason) =>
                $"We've reviewed your edit request for the challenge. Before we can proceed, we need some clarification: {reason}";
        }

        // ================================================================
        // FACULTY SPECIALIST / facultySpecialist NOTIFICATIONS
        // ================================================================

        public static class FacultySpecialist
        {
            public static string InvitedTitle() => "Challenge Invitation";
            public static string InvitedMessage(string challengeTitle, string? partnerName, string? description) =>
                $"You have been invited to participate in the challenge '{challengeTitle}' " +
                $"from {partnerName ?? "a community partner"}. " +
                $"{(string.IsNullOrEmpty(description) ? "" : $"Description: {description}")}";

            public static string RespondedTitle(string responseText) => $"Faculty Specialist {responseText} Challenge Invitation";
            public static string RespondedMessage(string FacultySpecialistName, string challengeTitle, string responseText) =>
                $"Faculty Specialist {ResolveFacultySpecialistDisplayName(FacultySpecialistName)} has {responseText.ToLower()} the invitation for challenge '{challengeTitle}'.";
        }

        // ================================================================
        // PARTNER NOTIFICATIONS
        // ================================================================

        public static class Partner
        {
            public static string RegisteredTitle() => "New Partner Registration";
            public static string RegisteredMessage(string institutionName) =>
                $"A new community partner '{institutionName}' has registered and is being reviewed by the team.";

            public static string ApprovedTitle() => "Registration Complete";
            public static string ApprovedMessage(string institutionName) =>
                $"Welcome to RICHConnect! Your registration for '{institutionName}' has been processed and your account is now active. You can now submit challenges and collaborate with faculty specialists.";

            public static string RejectedTitle() => "Registration Update Required";
            public static string RejectedMessage(string institutionName, string reason) =>
                $"Thank you for your interest in RICHConnect. Your registration for '{institutionName}' requires some additional information or modifications. Please review the following feedback and update your information: {reason}";

            public static string CriticalUpdateTitle() => "Partner Profile Update - Important Fields Changed";
            public static string CriticalUpdateMessage(string institutionName, string fieldsChanged) =>
                $"Partner '{institutionName}' has updated important profile information: {fieldsChanged}";
        }

        // ================================================================
        // THEME NOTIFICATIONS
        // ================================================================

        public static class Theme
        {
            public static string SubmittedTitle() => "New Theme Submitted";
            public static string SubmittedMessage(string themeTitle) =>
                $"A new theme '{themeTitle}' has been submitted and is being reviewed by the team.";

            public static string ApprovedTitle() => "Theme Published";
            public static string ApprovedMessage(string themeTitle) =>
                $"Your theme '{themeTitle}' has been published and is now available in the system. Challenges can now be created under this theme.";

            public static string RejectedTitle() => "Theme Needs Updates";
            public static string RejectedMessage(string themeTitle, string reason) =>
                $"Thank you for submitting the theme '{themeTitle}'. To publish it, please review the following feedback: {reason}";
        }

        // ================================================================
        // RESEARCH FIELD NOTIFICATIONS
        // ================================================================

        public static class ResearchField
        {
            public static string SubmittedTitle() => "New Research Field Submitted";
            public static string SubmittedMessage(string fieldName) =>
                $"A new research field '{fieldName}' has been submitted and is being reviewed by the team.";

            public static string ApprovedTitle() => "Research Field Added";
            public static string ApprovedMessage(string fieldName) =>
                $"Your research field '{fieldName}' has been added to the system and is now available for use in challenges and projects.";

            public static string RejectedTitle() => "Research Field Needs Updates";
            public static string RejectedMessage(string fieldName, string reason) =>
                $"Thank you for submitting the research field '{fieldName}'. To add it to the system, please review the following feedback: {reason}";
        }

        // ================================================================
        // R&D PROJECT NOTIFICATIONS
        // ================================================================

        public static class RDProject
        {
            public static string SubmittedTitle() => "New R&D Project Submitted";
            public static string SubmittedMessage(string projectTitle) =>
                $"A new R&D project '{projectTitle}' has been submitted and is being reviewed by the team.";

            public static string ApprovedTitle() => "R&D Project Ready for Matching";
            public static string ApprovedMessage(string projectTitle) =>
                $"Great news! Your R&D project '{projectTitle}' has been reviewed and is now ready for faculty specialist matching. You'll be notified when faculty specialists show interest.";

            public static string RejectedTitle() => "R&D Project Needs Updates";
            public static string RejectedMessage(string projectTitle, string reason) =>
                $"Thank you for submitting your R&D project '{projectTitle}'. To move forward, please review the following feedback and update your project: {reason}";

            public static string MatchedTitle() => "R&D Project Matched with Professors";
            public static string MatchedMessageAdmin(string projectTitle, int count, string FacultySpecialistNames) =>
                $"R&D project '{projectTitle}' has been matched with {count} facultySpecialist(s): {FacultySpecialistNames}.";
            public static string MatchedMessagePartner(string projectTitle, int count, string FacultySpecialistNames) =>
                $"Exciting news! Your R&D project '{projectTitle}' has been successfully matched with {count} facultySpecialist(s): {FacultySpecialistNames}. You can now collaborate on this project.";

            public static string FacultySpecialistInvitedTitle() => "R&D Project Invitation";
            public static string FacultySpecialistInvitedMessage(string projectTitle, string? description) =>
                $"You have been invited to participate in the R&D project '{projectTitle}'. " +
                $"{(string.IsNullOrEmpty(description) ? "" : $"Description: {description}")}";

            public static string FacultySpecialistRespondedTitle(string responseText) => 
                $"Faculty Specialist {responseText} R&D Project Invitation";
            public static string FacultySpecialistRespondedMessage(string FacultySpecialistName, string projectTitle, string responseText) =>
                $"Faculty Specialist {ResolveFacultySpecialistDisplayName(FacultySpecialistName)} has {responseText.ToLower()} the invitation for R&D project '{projectTitle}'.";

            public static string EditRequestedTitle() => "R&D Project Edit Request Submitted";
            public static string EditRequestedMessage(string projectTitle, string reason) =>
                $"A Community Partner has requested changes to their R&D project '{projectTitle}'. " +
                $"Please review and apply the requested changes. Reason: {reason}";

            public static string EditRequestApprovedTitle() => "R&D Project Updates Applied";
            public static string EditRequestApprovedMessage(string? adminResponse) =>
                $"Your requested changes to the R&D project have been processed and applied." +
                (!string.IsNullOrEmpty(adminResponse) ? $" Team note: {adminResponse}" : "");

            public static string EditRequestRejectedTitle() => "R&D Project Edit Request - Additional Information Needed";
            public static string EditRequestRejectedMessage(string reason) =>
                $"We've reviewed your edit request for the R&D project. Before we can proceed, we need some clarification: {reason}";
        }
    }
}
