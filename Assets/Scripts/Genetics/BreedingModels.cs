public class BreedingRuntime
{
    public string ChildId { get; }
    public string ParentAId { get; }
    public string ParentBId { get; }
    public int DaysRemaining { get; set; }

    public BreedingRuntime(string childId, string parentAId, string parentBId, int durationDays)
    {
        ChildId = childId;
        ParentAId = parentAId;
        ParentBId = parentBId;
        DaysRemaining = durationDays;
    }
}