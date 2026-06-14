using UnityEngine;

public class TestPopup : PopupBase
{
    protected override void Refresh()
    {
        // Aqui no futuro é onde você buscaria os dados no SaveManager 
        // e atualizaria os textos da tela (ex: textoDia.text = "Dia " + SaveManager.Instance.CurrentDay)
        Debug.Log($"[{gameObject.name}] Dados atualizados na tela!");
    }
}