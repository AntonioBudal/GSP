using UnityEngine;
using TMPro;
using System.Collections;

public class UI_GuidanceHUD : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TextMeshProUGUI _txtLore;
    [SerializeField] private TextMeshProUGUI _txtDirective;
    [SerializeField] private float _fadeSpeed = 2f;

    private void Start()
    {
        // Garante que nasce invisível
        _canvasGroup.alpha = 0f;

        // Assina o evento do Core assim que o Bootstrap estiver pronto
        if (GameBootstrap.Instance != null && GameBootstrap.Instance.Onboarding != null)
        {
            GameBootstrap.Instance.Onboarding.OnObjectiveUpdated += HandleObjectiveUpdated;
            // Pede o primeiro texto
            GameBootstrap.Instance.Onboarding.StartOnboarding();
        }
    }

    private void HandleObjectiveUpdated(string lore, string directive)
    {
        if (string.IsNullOrEmpty(lore))
        {
            // Concluído, esconde a HUD
            StopAllCoroutines();
            StartCoroutine(FadeHUD(0f));
            return;
        }

        _txtLore.text = lore;
        _txtDirective.text = directive;
        
        StopAllCoroutines();
        StartCoroutine(FadeHUD(1f));
    }

    private IEnumerator FadeHUD(float targetAlpha)
    {
        while (!Mathf.Approximately(_canvasGroup.alpha, targetAlpha))
        {
            _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, targetAlpha, Time.unscaledDeltaTime * _fadeSpeed);
            yield return null;
        }
    }

    private void OnDestroy()
    {
        if (GameBootstrap.Instance != null && GameBootstrap.Instance.Onboarding != null)
        {
            GameBootstrap.Instance.Onboarding.OnObjectiveUpdated -= HandleObjectiveUpdated;
        }
    }
}