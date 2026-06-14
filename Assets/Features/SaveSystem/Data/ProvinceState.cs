using System;

[Serializable]
public enum ProvinceStatus
{
    Hidden,
    Frontier,
    Revealed
}

public class ProvinceState
{
    public string id;
    public ProvinceStatus status;

    public ProvinceState() { }

    public ProvinceState(string id, ProvinceStatus status)
    {
        this.id = id;
        this.status = status;
    }
}