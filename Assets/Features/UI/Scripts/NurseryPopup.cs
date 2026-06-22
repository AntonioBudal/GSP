using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class NurseryPopup : PopupBase
{
    [Header("UI Elements")]
    [SerializeField] private Transform listContainer;
    [SerializeField] private RavenListEntry entryPrefab;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button confirmButton;

    private readonly List<string> selectedParents = new List<string>();

    protected override void BindData()
    {
        ClearContainer(listContainer);
        selectedParents.Clear();
        UpdateUIState();

        if (currentPayload is IEnumerable<RavenData> availableRavens)
        {
            foreach (var raven in availableRavens.Where(r => r.state == RavenState.Available))
            {
                var entry = Instantiate(entryPrefab, listContainer);
                entry.Setup(raven, OnRavenToggled);
            }
        }
    }

    private void OnRavenToggled(string ravenId)
    {
        if (selectedParents.Contains(ravenId))
        {
            selectedParents.Remove(ravenId);
        }
        else
        {
            if (selectedParents.Count >= 2)
                return;

            selectedParents.Add(ravenId);
        }

        UpdateUIState();
    }

    private void UpdateUIState()
    {
        if (statusText != null)
            statusText.text = $"Pais selecionados: {selectedParents.Count}/2";

        if (confirmButton != null)
            confirmButton.interactable = (selectedParents.Count == 2);
    }


    

    public void OnConfirmClicked()
    {
        if (selectedParents.Count != 2)
            return;

        bool success = NurseryManager.Instance.StartBreeding(selectedParents[0], selectedParents[1]);

        if (success)
        {
            Close();
        }
        else
        {
            if (statusText != null)
                statusText.text = "<color=red>Falha ao iniciar reprodução.</color>";
        }
    }
}