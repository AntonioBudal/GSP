using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class TrainingPopup : PopupBase
{
    [Header("UI Elements")]
    [SerializeField] private Transform listContainer;
    [SerializeField] private RavenListEntry entryPrefab;

    [Header("Training")]
    [SerializeField] private TrainingType currentSelectedTraining = TrainingType.Speed;

    protected override void BindData()
    {
        ClearContainer(listContainer);

        if (currentPayload is IEnumerable<RavenData> availableRavens)
        {
            foreach (var raven in availableRavens.Where(r => r.state == RavenState.Available))
            {
                var entry = Instantiate(entryPrefab, listContainer);
                entry.Setup(raven, OnRavenSelected);
            }
        }
    }

    private void OnRavenSelected(string ravenId)
    {
        bool success = TrainingManager.Instance.StartTraining(ravenId, currentSelectedTraining);

        if (success)
        {
            BindData();
        }
    }

    

    public void SetTrainingType(TrainingType trainingType)
    {
        currentSelectedTraining = trainingType;
        BindData();
    }
}