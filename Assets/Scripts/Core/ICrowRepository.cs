// Assets/Scripts/Core/ICrowRepository.cs
using System.Collections.Generic;

public interface ICrowRepository
{
    Crow GetCrow(string id);
    
    // A assinatura que libera o acesso para o SaveManager
    IEnumerable<Crow> GetAllCrows();
}