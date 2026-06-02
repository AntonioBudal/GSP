using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class UIWindow : MonoBehaviour
{
    private CanvasGroup _canvasGroup;
    private float _fadeDuration = 0.2f;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void ShowImmediate()
    {
        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }

    public void HideImmediate()
    {
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    public void Show()
    {
        StopAllCoroutines();
        StartCoroutine(Fade(1f, true));
    }

    public void Hide()
    {
        StopAllCoroutines();
        StartCoroutine(Fade(0f, false));
    }

    public virtual bool TryClose()
    {
        return true; // Por padrão, todas as janelas fecham livremente
    }

    private IEnumerator Fade(float targetAlpha, bool interactable)
    {
        _canvasGroup.interactable = interactable;
        _canvasGroup.blocksRaycasts = interactable;

        while (!Mathf.Approximately(_canvasGroup.alpha, targetAlpha))
        {
            // Substituímos Time.deltaTime por Time.unscaledDeltaTime
            _canvasGroup.alpha = Mathf.MoveTowards(_canvasGroup.alpha, targetAlpha, Time.unscaledDeltaTime / _fadeDuration);
            yield return null;
        }
    }
}