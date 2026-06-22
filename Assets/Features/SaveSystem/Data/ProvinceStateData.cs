using System;

[Serializable]
public enum ProvinceStatus
{
    Hidden,
    Frontier,
    Revealed
}

[Serializable]
public class ProvinceStateData
{
    public string provinceId;
    public ProvinceStatus status;

    public ProvinceStateData() { }

    public ProvinceStateData(string provinceId, ProvinceStatus status)
    {
        this.provinceId = provinceId;
        this.status = status;
    }

    
}