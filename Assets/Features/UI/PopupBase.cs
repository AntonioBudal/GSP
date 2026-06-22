using UnityEngine;

public abstract class PopupBase : MonoBehaviour
{
    [Header("Configurações Base")]
    public PopupType popupType;

    public virtual void RefreshView()
    {
        // Só gasta processamento se o popup estiver visível na tela
        if (gameObject.activeSelf && currentPayload != null)
        {
            BindData();
        }
    }
    protected object currentPayload;

    public virtual void Open(object dataPayload = null)
    {
        currentPayload = dataPayload;
        gameObject.SetActive(true);
        BindData(); // Força a renderização baseada no payload recebido
    }

    public virtual void Close()
    {
        gameObject.SetActive(false);
        currentPayload = null; // Limpa o estado temporário
    }

    // Contrato obrigatório: como o popup consome o currentPayload
    protected abstract void BindData();

    // Utilitário para limpar listas
    protected void ClearContainer(Transform container)
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }
}