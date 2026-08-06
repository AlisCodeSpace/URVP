namespace FEA.URVP.Domain.Entities.StudentProfiles;

public sealed class DayAvailability
{
    public string Day { get; set; } = null!;

    public List<string> Slots { get; set; } = [];
}
