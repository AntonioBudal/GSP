using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Popups Registrados")]
    // Arraste todos os objetos de popup da cena para esta lista
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

        // Garante que o jogo comece com todas as telas fechadas
        CloseAllPopups();
    }

    private void Update()
    {
        // Atalho para fechar qualquer popup aberto usando ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseCurrentPopup();
        }

        
    }

    public void OpenPopup(PopupType type)
    {
        // Se já tem um popup aberto, fecha ele primeiro
        if (currentPopup != null)
        {
            currentPopup.Close();
        }

        // Busca o novo popup na lista
        PopupBase popupToOpen = registeredPopups.FirstOrDefault(p => p.popupType == type);

        if (popupToOpen != null)
        {
            currentPopup = popupToOpen;
            currentPopup.Open();
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
            Debug.Log($"[UIManager] Popup fechado: {currentPopup.popupType}");
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