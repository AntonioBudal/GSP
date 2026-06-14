using UnityEngine;

public abstract class PopupBase : MonoBehaviour
{
    [Header("Configurações Base")]
    public PopupType popupType;

    // Método virtual permite que as classes filhas adicionem comportamentos extras se precisarem
    public virtual void Open()
    {
        gameObject.SetActive(true);
        Refresh(); // Sempre que abre, força a atualização com os dados mais recentes do Save
    }

    public virtual void Close()
    {
        gameObject.SetActive(false);
    }

    // Contrato obrigatório: todo popup precisa saber desenhar seus próprios dados
    protected abstract void Refresh();
}