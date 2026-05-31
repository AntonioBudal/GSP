using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class UI_TutorialHUD : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TextMeshProUGUI _txtObjective;
    [SerializeField] private float _fadeSpeed = 3f;

   private IEnumerator Start()
    {
        _canvasGroup.alpha = 0f;

        // 1. Aguarda o Bootstrap
        while (GameBootstrap.Instance == null || GameBootstrap.Instance.Onboarding == null)
        {
            yield return null;
        }

        // 2. Se inscreve no evento
        GameBootstrap.Instance.Onboarding.OnObjectiveUpdated += HandleObjectiveUpdated;

        // 3. A MÁGICA: Espera meio segundo antes de disparar.
        // Isso garante que TODOS os outros scripts do jogo já terminaram de se inscrever
        // e ainda cria um "respiro" dramático de meio segundo quando o jogo abre.
        yield return new WaitForSecondsRealtime(0.5f);

        // 4. Puxa o gatilho
        GameBootstrap.Instance.Onboarding.StartOnboarding();
    }

    private void HandleObjectiveUpdated(string lore, string directive)
    {
        StopAllCoroutines();

        if (string.IsNullOrEmpty(directive))
        {
            StartCoroutine(FadeTo(0f)); // O tutorial acabou
            return;
        }

        // Formatação diegética do checkbox
        _txtObjective.text = $"OBJETIVO\n□ {directive}";
        StartCoroutine(FadeTo(1f));
    }

    private IEnumerator FadeTo(float targetAlpha)
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