using System;

[Serializable]
public enum RavenState
{
    Available,
    Exploring,
    Training,
    Breeding,
    Dead
}

[Serializable]
public class RavenData
{
    public string id;
    public int speed;
    public int lifespan;
    public int focus;
    public RavenState state;

    // Construtor vazio necessário para a serialização
    public RavenData() { }

    public RavenData(string id, int speed, int lifespan, int focus)
    {
        this.id = id;
        this.speed = speed;
        this.lifespan = lifespan;
        this.focus = focus;
        this.state = RavenState.Available;
    }
}