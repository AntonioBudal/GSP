using System;

[Serializable]
public class ExpeditionData
{
    public string ravenId;
    public string targetProvinceId;
    public int remainingDays;

    public ExpeditionData() { }

    public ExpeditionData(string ravenId, string targetProvinceId, int remainingDays)
    {
        this.ravenId = ravenId;
        this.targetProvinceId = targetProvinceId;
        this.remainingDays = remainingDays;
    }
}