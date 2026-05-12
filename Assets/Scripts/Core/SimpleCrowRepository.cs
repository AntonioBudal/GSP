using System.Collections.Generic;
using System.Linq;
using Corvus.Core.SaveSystem;

public class SimpleCrowRepository : ICrowRepository
{
    private readonly List<Crow> _crows = new();

    public SimpleCrowRepository() { }

    public SimpleCrowRepository(List<CrowSaveData> saveData)
    {
        if (saveData == null) return;

        foreach (var data in saveData)
        {
            var genetics = new GeneticSeed(data.Traits);

            var crow = new Crow(
                data.ID,
                data.Speed,
                data.Focus,
                data.Resilience,
                data.Lifespan,
                genetics
            );

            crow.CurrentState = data.CurrentState;
            crow.Role = data.Role;

            _crows.Add(crow);
        }
    }

    public void AddCrow(Crow crow) => _crows.Add(crow);

    public IEnumerable<Crow> GetAllCrows() => _crows;

    public Crow GetCrow(string id) =>
        _crows.FirstOrDefault(c => c.ID == id);
}