using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class RavenListEntry : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private Button selectButton;
    [SerializeField] private Button inspectButton; // <--- NOVO CAMPO AQUI
    [SerializeField] private GameObject selectionHighlight;

    private string currentRavenId;
    private Action<string> onToggledCallback;

    public void Setup(RavenData data, Action<string> onToggled)
    {
        currentRavenId = data.id;
        onToggledCallback = onToggled;
        
        infoText.text = $"ID: {data.id} | Vel: {data.speed} | Foco: {data.focus}";
        
        SetSelected(false);

        // Conecta o botão principal (Selecionar)
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(OnClick);

        // CONECTA O BOTÃO [ i ] DIRETO NO CÓDIGO (Ignorando o Inspector da Unity!)
        if (inspectButton != null)
        {
            inspectButton.onClick.RemoveAllListeners();
            inspectButton.onClick.AddListener(OnInspectClicked);
        }
    }

    public void SetSelected(bool isSelected)
    {
        if (selectionHighlight != null)
        {
            selectionHighlight.SetActive(isSelected);
        }
    }

    private void OnClick()
    {
        onToggledCallback?.Invoke(currentRavenId);
    }

    // O gatilho blindado que a Unity tentou esconder de você:
    private void OnInspectClicked()
    {
        UIManager.Instance.OpenPopup(PopupType.RavenStats, currentRavenId);
    }
}