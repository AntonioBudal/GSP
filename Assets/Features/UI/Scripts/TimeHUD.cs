using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeHUD : MonoBehaviour
{
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private Button advanceDayButton;

    private void Start()
    {
        // 1. Conecta o clique do botão diretamente ao motor de tempo
        advanceDayButton.onClick.RemoveAllListeners();
        advanceDayButton.onClick.AddListener(() => TimeManager.Instance.AdvanceDay());

        // 2. Inscreve a própria HUD para se reescrever quando o tempo passar
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnDayAdvanced += UpdateHUDText;
        }

        UpdateHUDText(); // Força a escrita do "Dia 1" ao dar Play
    }

    private void OnDestroy()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnDayAdvanced -= UpdateHUDText;
        }
    }

    private void UpdateHUDText()
    {
        if (dayText != null && TimeManager.Instance != null)
        {
            dayText.text = $"Dia {TimeManager.Instance.CurrentDay}";
        }
    }
}