using UnityEngine;
using TMPro; // TextMeshPro

public class HUDClockController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _dayText;

    private void Start()
    {
        // Aguarda um frame para garantir que o Bootstrap já instanciou o Clock
        Invoke(nameof(SubscribeToClock), 0.1f);
    }

    private void SubscribeToClock()
    {
        if (GameBootstrap.Instance != null && GameBootstrap.Instance.Clock != null)
        {
            GameBootstrap.Instance.Clock.OnDayEnded += UpdateDisplay;
            UpdateDisplay(0); // Força a atualização inicial
        }
        else
        {
            Debug.LogError("[HUDClock] Bootstrap ou Clock não encontrados.");
        }
    }

    private void UpdateDisplay(int day)
    {
        _dayText.text = $"DIA {day}";
    }

    private void OnDestroy()
    {
        if (GameBootstrap.Instance != null && GameBootstrap.Instance.Clock != null)
        {
            GameBootstrap.Instance.Clock.OnDayEnded -= UpdateDisplay;
        }
    }
}