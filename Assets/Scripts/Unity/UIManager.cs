using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Janelas Principais")]
    public UIWindow windowTemple;
    public UIWindow windowMap;

    [Header("Popups")]
    public UI_ExpeditionPopup popupExpedition;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Estado inicial da interface
        if (windowMap != null) windowMap.HideImmediate();
        if (windowTemple != null) windowTemple.ShowImmediate();
    }

    public void OpenTemple()
    {
        if (windowMap != null) windowMap.Hide();
        if (windowTemple != null) windowTemple.Show();
    }

    public void OpenMap()
    {
        if (windowTemple != null) windowTemple.Hide();
        if (windowMap != null) windowMap.Show();
    }

    // O método que o C# não estava achando agora está aqui, devidamente isolado
    public void OpenExpeditionPopup(string regionId)
    {
        if (popupExpedition != null)
        {
            popupExpedition.SetupAndShow(regionId);
        }
    }
}