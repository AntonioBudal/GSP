using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class UI_NarrativePromptHUD : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TextMeshProUGUI _txtLore;
    
    [Header("Configurações de Exibição")]
    [Tooltip("Tempo em segundos que a mensagem fica na tela antes de sumir.")]
    [SerializeField] private float _displayDuration = 16f;
    [SerializeField] private float _fadeSpeed = 1.5f;

    private IEnumerator Start()
    {
        _canvasGroup.alpha = 0f;

        // Aguarda o Bootstrap
        while (GameBootstrap.Instance == null || GameBootstrap.Instance.Onboarding == null)
        {
            yield return null;
        }

        // Este apenas escuta (não chama o StartOnboarding para evitar mensagem dupla)
        GameBootstrap.Instance.Onboarding.OnObjectiveUpdated += HandleLoreUpdated;
    }

    private void HandleLoreUpdated(string lore, string directive)
    {
        if (string.IsNullOrEmpty(lore)) return; // Se for vazio, não faz nada

        _txtLore.text = lore;
        
        StopAllCoroutines();
        StartCoroutine(Routine_DisplayLore());
    }

    private IEnumerator Routine_DisplayLore()
    {
        // 1. Fade In
        while (_canvasGroup.alpha < 1f)
        {
            _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, 1f, Time.unscaledDeltaTime * _fadeSpeed);
            yield return null;
        }

        // 2. Aguarda o tempo de leitura
        yield return new WaitForSecondsRealtime(_displayDuration);

        // 3. Fade Out
        while (_canvasGroup.alpha > 0f)
        {
            _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, 0f, Time.unscaledDeltaTime * _fadeSpeed);
            yield return null;
        }
    }

    private void OnDestroy()
    {
        if (GameBootstrap.Instance != null && GameBootstrap.Instance.Onboarding != null)
        {
            GameBootstrap.Instance.Onboarding.OnObjectiveUpdated -= HandleLoreUpdated;
        }
    }
}