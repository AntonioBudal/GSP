using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CrowPopup : PopupBase
{
    [Header("UI References")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text ageText;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private Button closeButton;

    protected override void BindData()
    {
        if (currentPayload is string ravenId)
        {
            // Puxa o corvo fresco direto do Manager encapsulado
            RavenData raven = RavenManager.Instance.GetRavenById(ravenId);
            
            if (raven == null)
            {
                Debug.LogWarning($"[CrowPopup] O corvo {ravenId} não existe mais (provavelmente faleceu).");
                Close();
                return;
            }

            titleText.text = $"Ficha: {raven.id}";
            statusText.text = TranslateStatus(raven.state);

            // UX Visual: Deixa a idade amarela se estiver perto de morrer, e vermelha se estiver fazendo hora extra
            string ageColor = raven.age >= raven.lifespan ? "#FF0000" : raven.age >= (raven.lifespan - 2) ? "#FFD700" : "#FFFFFF";
            ageText.text = $"Idade: <color={ageColor}>{raven.age}</color> / {raven.lifespan} Dias";

            statsText.text = $"Velocidade de Voo: {raven.speed}\nFoco Estratégico: {raven.focus}";
        }
        else
        {
            Debug.LogError("[CrowPopup] Payload inválido. Esperado string (ravenId).");
        }

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(Close);
    }

    private string TranslateStatus(RavenState state)
    {
        switch (state)
        {
            case RavenState.Available: return "<color=#00FF00>Disponível no Ninho</color>";
            case RavenState.Exploring: return "<color=#00BFFF>Em Expedição</color>";
            case RavenState.Training: return "<color=#FFA500>Nas Instalações de Treino</color>";
            case RavenState.Breeding: return "<color=#FF69B4>No Berçário</color>";
            case RavenState.Dead: return "<color=#FF0000>Caído</color>";
            default: return state.ToString();
        }
    }
}