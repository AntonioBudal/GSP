using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDTimeView : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _dayText;
    [SerializeField] private Button _nextDayButton;

    // Expomos o botão para o Presenter assinar os callbacks
    public Button NextDayButton => _nextDayButton;

    public void UpdateDayText(int currentDay)
    {
        _dayText.text = $"Dia {currentDay}";
    }

    public void SetInteractable(bool state)
    {
        _nextDayButton.interactable = state;
    }
}