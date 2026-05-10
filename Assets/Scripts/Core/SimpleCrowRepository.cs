// Assets/Scripts/Core/Entities/SimpleCrowRepository.cs
using System.Collections.Generic;
using System.Linq;

public class SimpleCrowRepository : ICrowRepository
{
    private readonly List<Crow> _crows = new List<Crow>();

    // Métodos utilitários para o Bootstrap conseguir injetar os dados
    public void AddCrow(Crow crow) => _crows.Add(crow);
    public IEnumerable<Crow> GetAllCrows() => _crows;

    // A implementação obrigatória da sua interface
    public Crow GetCrow(string id) => _crows.FirstOrDefault(c => c.ID == id);
}