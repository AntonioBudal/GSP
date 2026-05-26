using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_MapPawn : MonoBehaviour
{
    [Header("Visuais")]
    [SerializeField] private Image _missionIcon; // Pode trocar a cor ou sprite dependendo da missão
    [SerializeField] private TextMeshProUGUI _progressText;

    public string CrowId { get; private set; }

    public void Setup(string crowId, MissionType mission)
    {
        CrowId = crowId;
        _progressText.text = "0d";
        
        // Suco visual: Batedor fica branco/cinza, Evangelizador fica dourado
        if (_missionIcon != null)
        {
            _missionIcon.color = mission == MissionType.Reconhecimento ? Color.white : new Color(1f, 0.8f, 0.2f);
        }
    }

    public void UpdateProgress(int daysElapsed)
    {
        if (_progressText != null)
        {
            _progressText.text = $"{daysElapsed}d";
        }
    }
}