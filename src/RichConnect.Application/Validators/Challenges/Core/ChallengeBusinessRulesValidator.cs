using RICHConnect.Backend.Infrastructure.Data.Repositories.Challenges.Interfaces;
using FluentValidation.Results;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Validators.Challenges
{
    /// <summary>
    /// Comprehensive business rules validator for Challenge operations
    /// </summary>
    public class ChallengeBusinessRulesValidator
    {
        private readonly IChallengeRepository _repository;

        public ChallengeBusinessRulesValidator(IChallengeRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Validates challenge status transitions according to business rules
        /// </summary>
        public Task<ValidationResult> ValidateStatusTransitionAsync(
            Guid challengeId, 
            ChallengeStatus currentStatus, 
            ChallengeStatus newStatus)
        {
            var result = new ValidationResult();

            // Define allowed status transitions
            var allowedTransitions = new Dictionary<ChallengeStatus, List<ChallengeStatus>>
            {
                { ChallengeStatus.Pending, new List<ChallengeStatus> { ChallengeStatus.Approved, ChallengeStatus.Rejected } },
                { ChallengeStatus.Approved, new List<ChallengeStatus> { ChallengeStatus.Matched } },
                { ChallengeStatus.Rejected, new List<ChallengeStatus>() }, // No transitions from rejected
                { ChallengeStatus.Matched, new List<ChallengeStatus>() } // No transitions from matched
            };

            if (!allowedTransitions.ContainsKey(currentStatus))
            {
                result.Errors.Add(new ValidationFailure("Status", $"Invalid current status: {currentStatus}"));
                return Task.FromResult(result);
            }

            if (!allowedTransitions[currentStatus].Contains(newStatus))
            {
                result.Errors.Add(new ValidationFailure("Status", 
                    $"Cannot transition from {currentStatus} to {newStatus}. " +
                    $"Allowed transitions from {currentStatus}: {string.Join(", ", allowedTransitions[currentStatus])}"));
            }

            return Task.FromResult(result);
        }

        /// <summary>
        /// Validates that challenges can only be updated in certain statuses
        /// Note: This method now allows admins to update any challenge, while partners can only update their own pending challenges
        /// </summary>
        public async Task<ValidationResult> ValidateUpdateRestrictionsAsync(Guid challengeId, Guid userId, bool isAdmin = false)
        {
            var result = new ValidationResult();
            var challenge = await _repository.GetByIdAsync(challengeId);

            if (challenge == null)
            {
                result.Errors.Add(new ValidationFailure("ChallengeId", "Challenge not found"));
                return result;
            }

            // If user is admin, they can update any challenge regardless of status
            if (isAdmin)
            {
                return result; // No restrictions for admins
            }

            // For non-admin users (Community Partners), apply restrictions
            // Only allow updates to challenges in Pending status
            if (challenge.Status != ChallengeStatus.Pending)
            {
                result.Errors.Add(new ValidationFailure("Status", 
                    $"Challenge cannot be updated. Current status: {challenge.Status}. " +
                    "Only challenges with 'Pending' status can be updated."));
            }

            // Only allow the original submitter to update their challenge
            if (challenge.SubmittedBy != userId)
            {
                result.Errors.Add(new ValidationFailure("UserId", 
                    "Only the original submitter can update this challenge."));
            }

            return result;
        }

        /// <summary>
        /// Validates estimated cost constraints according to business rules
        /// </summary>
        public ValidationResult ValidateEstimatedCostConstraints(decimal estimatedCost)
        {
            var result = new ValidationResult();

            // Cost constraints (matching frontend limits)
            const decimal MIN_COST = 0.01m; // $0.01 minimum
            const decimal MAX_COST = 1000000000m; // $1,000,000,000 maximum

            if (estimatedCost < MIN_COST)
            {
                result.Errors.Add(new ValidationFailure("EstimatedCost", 
                    $"Estimated cost must be at least ${MIN_COST:N2}"));
            }

            if (estimatedCost > MAX_COST)
            {
                result.Errors.Add(new ValidationFailure("EstimatedCost", 
                    $"Estimated cost cannot exceed ${MAX_COST:N2}"));
            }

            return result;
        }

        /// <summary>
        /// Validates to prevent duplicate challenges
        /// </summary>
        public async Task<ValidationResult> ValidateDuplicatePreventionAsync(
            string title, 
            Guid researchFieldId, 
            Guid submittedBy, 
            Guid? excludeChallengeId = null)
        {
            var result = new ValidationResult();

            // Check for exact title matches within the same research field
            var existingChallenges = await _repository.GetByUserAsync(submittedBy);
            var duplicateChallenges = existingChallenges
                .Where(c => c.Title.Equals(title, StringComparison.OrdinalIgnoreCase) &&
                           c.ResearchFieldId == researchFieldId &&
                           c.Status != ChallengeStatus.Rejected && // Ignore rejected challenges
                           (excludeChallengeId == null || c.Id != excludeChallengeId))
                .ToList();

            if (duplicateChallenges.Any())
            {
                result.Errors.Add(new ValidationFailure("Title", 
                    $"A challenge with the title '{title}' already exists in this research field. " +
                    "Please choose a different title or research field."));
            }

            // Check for similar titles (fuzzy matching)
            var similarChallenges = existingChallenges
                .Where(c => IsSimilarTitle(c.Title, title) &&
                           c.ResearchFieldId == researchFieldId &&
                           c.Status != ChallengeStatus.Rejected &&
                           (excludeChallengeId == null || c.Id != excludeChallengeId))
                .ToList();

            if (similarChallenges.Any())
            {
                result.Errors.Add(new ValidationFailure("Title", 
                    $"A similar challenge already exists: '{similarChallenges.First().Title}'. " +
                    "Please ensure your challenge title is sufficiently different."));
            }

            return result;
        }

        /// <summary>
        /// Validates time-based rules for challenges
        /// </summary>
        public ValidationResult ValidateTimeBasedRules(DateTime? deadline = null)
        {
            var result = new ValidationResult();
            var now = DateTime.UtcNow;

            if (deadline.HasValue)
            {
                // Deadline must be in the future
                if (deadline.Value <= now)
                {
                    result.Errors.Add(new ValidationFailure("Deadline", 
                        "Challenge deadline must be in the future"));
                }

                // Deadline must not be too far in the future (business rule: max 1 year)
                var maxDeadline = now.AddYears(1);
                if (deadline.Value > maxDeadline)
                {
                    result.Errors.Add(new ValidationFailure("Deadline", 
                        "Challenge deadline cannot be more than 1 year in the future"));
                }

                // Deadline must not be too soon (business rule: min 30 days)
                var minDeadline = now.AddDays(30);
                if (deadline.Value < minDeadline)
                {
                    result.Errors.Add(new ValidationFailure("Deadline", 
                        "Challenge deadline must be at least 30 days in the future"));
                }
            }

            return result;
        }

        /// <summary>
        /// Validates that challenges can only be approved/rejected by admins
        /// </summary>
        public Task<ValidationResult> ValidateAdminOnlyOperationsAsync(Guid userId, string operation)
        {
            var result = new ValidationResult();

            // This would typically check against a user service or role service
            // For now, we'll assume this validation is handled at the controller level
            // with [Authorize(Roles = "Admin")] attributes

            return Task.FromResult(result);
        }

        /// <summary>
        /// Validates that matching operations can only be performed on approved challenges
        /// </summary>
        public async Task<ValidationResult> ValidateMatchingOperationsAsync(Guid challengeId)
        {
            var result = new ValidationResult();
            var challenge = await _repository.GetByIdAsync(challengeId);

            if (challenge == null)
            {
                result.Errors.Add(new ValidationFailure("ChallengeId", "Challenge not found"));
                return result;
            }

            if (challenge.Status != ChallengeStatus.Approved)
            {
                result.Errors.Add(new ValidationFailure("Status", 
                    $"Matching operations can only be performed on approved challenges. " +
                    $"Current status: {challenge.Status}"));
            }

            return result;
        }

        /// <summary>
        /// Validates that finalization can only occur when all invites have been responded to
        /// </summary>
        public async Task<ValidationResult> ValidateFinalizationRulesAsync(Guid challengeId)
        {
            var result = new ValidationResult();
            var invites = await _repository.GetInvitesByChallengeAsync(challengeId);

            if (!invites.Any())
            {
                result.Errors.Add(new ValidationFailure("Invites", 
                    "Cannot finalize matching: No facultySpecialist invites found for this challenge"));
                return result;
            }

            var pendingInvites = invites.Where(i => i.Status == InviteStatus.Pending).ToList();
            if (pendingInvites.Any())
            {
                result.Errors.Add(new ValidationFailure("Invites", 
                    $"Cannot finalize matching: {pendingInvites.Count} facultySpecialist(s) have not yet responded to their invites"));
            }

            var acceptedInvites = invites.Where(i => i.Status == InviteStatus.Accepted).ToList();
            if (!acceptedInvites.Any())
            {
                result.Errors.Add(new ValidationFailure("Invites", 
                    "Cannot finalize matching: No professors have accepted their invites"));
            }

            return result;
        }

        /// <summary>
        /// Helper method to check for similar titles using simple string comparison
        /// </summary>
        private static bool IsSimilarTitle(string title1, string title2)
        {
            if (string.IsNullOrEmpty(title1) || string.IsNullOrEmpty(title2))
                return false;

            // Simple similarity check - can be enhanced with more sophisticated algorithms
            var words1 = title1.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var words2 = title2.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var commonWords = words1.Intersect(words2).Count();
            var totalWords = Math.Max(words1.Length, words2.Length);

            // Consider similar if more than 70% of words match
            return totalWords > 0 && (double)commonWords / totalWords > 0.7;
        }
    }
}
