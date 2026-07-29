namespace RICHConnect.Backend.Application.Validators.Challenges
{
    /// <summary>
    /// Constants for challenge validation rules to ensure consistency across all validators
    /// </summary>
    public static class ChallengeValidationConstants
    {
        // Title validation
        public const int TITLE_MAX_LENGTH = 200;
        public const int TITLE_MIN_LENGTH = 1;

        // Description validation
        public const int DESCRIPTION_MAX_LENGTH = 2000;

        // Supporting document validation
        public const int SUPPORTING_DOCUMENT_URL_MAX_LENGTH = 512;
        public const string SUPPORTING_DOCUMENT_ALLOWED_EXTENSION = ".pdf";

        // Estimated cost validation (matching frontend limits)
        public const decimal MIN_ESTIMATED_COST = 0.01m; // $0.01 minimum
        public const decimal MAX_ESTIMATED_COST = 1000000000m; // $1,000,000,000 maximum

        // Rejection reason validation
        public const int REJECTION_REASON_MAX_LENGTH = 1000;
        public const int REJECTION_REASON_MIN_LENGTH = 10;

        // Matching validation
        public const int MAX_FACULTY_SPECIALISTS_PER_INVITE = 10;
        public const int MIN_FACULTY_SPECIALISTS_PER_INVITE = 1;

        // Time-based validation
        public const int MIN_DEADLINE_DAYS = 30;
        public const int MAX_DEADLINE_DAYS = 365;

        // Similarity threshold for duplicate detection
        public const double SIMILARITY_THRESHOLD = 0.7; // 70% word overlap
    }
}
