namespace RICHConnect.Backend.Domain.Enums
{
    // ──────────────────────────────────────────────────────────────
    // COMMUNITY & INSTITUTION ENUMS
    // ──────────────────────────────────────────────────────────────
    
    /// <summary>
    /// Institution.Sector (Industry sector)
    /// </summary>
    public enum InstitutionSector : byte
    {
        Technology = 0,
        Manufacturing = 1,
        Agriculture = 2,
        Retail = 3,
        Education = 4,
        Healthcare = 5,
        Finance = 6,
        Other = 7
    }

    /// <summary>
    /// Institution.Size (Institution size)
    /// </summary>
    public enum InstitutionSize : byte
    {
        OneToTen = 0,        // 1-10
        ElevenToFifty = 1,   // 11-50
        FiftyOneToHundred = 2, // 51-100
        HundredOneToFiveHundred = 3, // 101-500
        FiveHundredOneToThousand = 4, // 501-1000
        OverThousand = 5     // 1000+
    }

    /// <summary>
    /// Institution.AccreditationType (Type of accreditation)
    /// </summary>
    public enum AccreditationType : byte
    {
        ISO = 0,
        CE = 1,
        FDA = 2,
        UL = 3,
        NSF = 4,
        Other = 5
    }
}

// ──────────────────────────────────────────────────────────────
// EXTENSION METHODS
// ──────────────────────────────────────────────────────────────

namespace RICHConnect.Backend.Domain.Enums
{
    /// <summary>
    /// Extension methods for InstitutionSize enum
    /// </summary>
    public static class InstitutionSizeExtensions
    {
        public static string ToDisplayString(this InstitutionSize size)
        {
            return size switch
            {
                InstitutionSize.OneToTen => "1-10",
                InstitutionSize.ElevenToFifty => "11-50",
                InstitutionSize.FiftyOneToHundred => "51-100",
                InstitutionSize.HundredOneToFiveHundred => "101-500",
                InstitutionSize.FiveHundredOneToThousand => "501-1000",
                InstitutionSize.OverThousand => "1000+",
                _ => "1-10"
            };
        }

        public static InstitutionSize FromDisplayString(string? sizeString)
        {
            return sizeString switch
            {
                "1-10" => InstitutionSize.OneToTen,
                "11-50" => InstitutionSize.ElevenToFifty,
                "51-100" => InstitutionSize.FiftyOneToHundred,
                "101-500" => InstitutionSize.HundredOneToFiveHundred,
                "501-1000" => InstitutionSize.FiveHundredOneToThousand,
                "1000+" => InstitutionSize.OverThousand,
                _ => InstitutionSize.OneToTen
            };
        }
    }
}
