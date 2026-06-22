using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class ExpeditionPopup : PopupBase
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private Transform listContainer;
    [SerializeField] private RavenListEntry entryPrefab;
    [SerializeField] private Button confirmButton;

    private string targetProvinceId;
    private ProvinceConfig targetConfig;
    private string selectedRavenId = string.Empty;
    private Dictionary<string, RavenListEntry> activeEntries = new Dictionary<string, RavenListEntry>();

    protected override void BindData()
    {
        ClearContainer(listContainer);
        activeEntries.Clear();
        selectedRavenId = string.Empty;

        if (currentPayload is string provinceId)
        {
            targetProvinceId = provinceId;
            targetConfig = MapManager.Instance.GetProvinceConfig(targetProvinceId);
        }
        else
        {
            Debug.LogError("[ExpeditionPopup] Payload inválido.");
            return;
        }

        if (titleText != null && targetConfig != null)
        {
            // Exibe claramente o Foco exigido no título da tela
            titleText.text = $"Expedição: {targetConfig.provinceName} (Exige Foco {targetConfig.requiredFocus})";
        }

        var availableRavens = SaveManager.Instance.CurrentSave.ravens
            .Where(r => r.state == RavenState.Available).ToList();

        foreach (var raven in availableRavens)
        {
            var entry = Instantiate(entryPrefab, listContainer);
            entry.Setup(raven, OnEntryToggled);
            activeEntries.Add(raven.id, entry);
        }

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(OnConfirmClicked);

        UpdateUIState();
    }

    private void OnEntryToggled(string ravenId)
    {
        selectedRavenId = selectedRavenId == ravenId ? string.Empty : ravenId;
        UpdateUIState();
    }

    private void UpdateUIState()
    {
        foreach (var kvp in activeEntries)
        {
            kvp.Value.SetSelected(kvp.Key == selectedRavenId);
        }

        if (string.IsNullOrEmpty(selectedRavenId))
        {
            confirmButton.interactable = false;
            if (infoText != null) infoText.text = "Selecione um corvo para a jornada.";
            return;
        }

        var raven = RavenManager.Instance.GetRavenById(selectedRavenId);
        if (raven != null)
        {
            // A REGRA DE NEGÓCIO DO FOCO ATUANDO NA UX:
            bool hasEnoughFocus = raven.focus >= targetConfig.requiredFocus;

            confirmButton.interactable = hasEnoughFocus;

            if (infoText != null)
            {
                if (hasEnoughFocus)
                {
                    int duration = ExpeditionManager.Instance.GetEstimatedDuration(raven.speed);
                    infoText.text = $"Duração estimada: {duration} dias.\n<color=#00FF00>Alcance estratégico suficiente.</color>";
                }
                else
                {
                    infoText.text = $"<color=#FF0000>Foco Insuficiente!</color>\nEsta ave possui Foco {raven.focus}, mas a região exige {targetConfig.requiredFocus}.";
                }
            }
        }
    }

    private void OnConfirmClicked()
    {
        bool success = ExpeditionManager.Instance.SendExpedition(selectedRavenId, targetProvinceId);
        if (success) Close();
    }
}