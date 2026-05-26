// Assets/Scripts/Core/SystemRandom.cs
using System;

public class SystemRandom : IRandomSource
{
    private Random _random = new Random();
    
    public int Next(int min, int max) => _random.Next(min, max);
    
    // Método adicionado para cumprir o seu contrato
    public double NextDouble() => _random.NextDouble();
}