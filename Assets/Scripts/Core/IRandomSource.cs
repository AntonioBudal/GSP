// Assets/Scripts/Core/IRandomSource.cs
public interface IRandomSource
{
    int Next(int min, int max);
    double NextDouble();
}

// Assets/Scripts/Core/ICrowRepository.cs
public interface ICrowRepository
{
    Crow GetCrow(string id);
}