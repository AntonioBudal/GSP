using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Popups Registrados")]
    [SerializeField] private List<PopupBase> registeredPopups = new List<PopupBase>();

    private PopupBase currentPopup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        CloseAllPopups();
    }

    private void Start()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnDayAdvanced += BroadcastDayChange;
        }
    }

    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnDayAdvanced -= BroadcastDayChange;
        }
    }

    private void BroadcastDayChange()
    {
        // Se houver algum modal na cara do jogador, manda ele repuxar os dados
        currentPopup?.RefreshView();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseCurrentPopup();
        }
    }

    // Agora aceita um objeto genérico como payload de dados
    public void OpenPopup(PopupType type, object payload = null)
    {
        if (currentPopup != null)
        {
            currentPopup.Close();
        }

        PopupBase popupToOpen = registeredPopups.FirstOrDefault(p => p.popupType == type);

        if (popupToOpen != null)
        {
            currentPopup = popupToOpen;
            currentPopup.Open(payload); // Repassa o payload
            Debug.Log($"[UIManager] Popup aberto: {type}");
        }
        else
        {
            Debug.LogError($"[UIManager] Popup do tipo {type} não encontrado na lista!");
        }
    }

    public void CloseCurrentPopup()
    {
        if (currentPopup != null)
        {
            currentPopup.Close();
            currentPopup = null;
        }
    }

    private void CloseAllPopups()
    {
        foreach (var popup in registeredPopups)
        {
            popup.Close();
        }
        currentPopup = null;
    }
}