namespace RICHConnect.Backend.Domain.Enums
{
    // ──────────────────────────────────────────────────────────────
    // facultySpecialist ENUMS
    // ──────────────────────────────────────────────────────────────
    
    /// <summary>
    /// FacultySpecialist.Status
    /// </summary>
    public enum FacultySpecialistStatus : byte
    {
        Unavailable = 0,
        Available = 1
    }

    /// <summary>
    /// FacultySpecialist.Department
    /// </summary>
    public enum FacultySpecialistDepartment : byte
    {
        ComputerScience = 0,
        ElectricalEngineering = 1,
        MechanicalEngineering = 2,
        Biology = 3,
        Chemistry = 4,
        Physics = 5,
        Mathematics = 6,
        Business = 7,
        Medicine = 8,
        Other = 9
    }

    /// <summary>
    /// FacultySpecialist.AcademicRank
    /// </summary>
    public enum AcademicRank : byte
    {
        AssistantfacultySpecialist = 0,
        AssociatefacultySpecialist = 1,
        facultySpecialist = 2,
        DistinguishedfacultySpecialist = 3,
        Emeritus = 4,
        Adjunct = 5,
        Visiting = 6
    }
}
