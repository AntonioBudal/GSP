// Assets/Scripts/Core/IRandomSource.cs
public interface IRandomSource
{
    int Next(int min, int max);
    double NextDouble();
}
