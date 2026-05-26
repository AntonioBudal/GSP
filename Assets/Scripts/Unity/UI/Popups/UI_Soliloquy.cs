// Assets/Scripts/Unity/Narrative/UI_Soliloquy.cs
using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class UI_Soliloquy : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _soliloquyText;
    
    [Header("Juice Settings")]
    [SerializeField] private float _typingSpeed = 0.05f; // Segundos entre cada letra
    [SerializeField] private float _displayDuration = 4.0f; // Tempo que o texto fica na tela após digitar
    [SerializeField] private float _fadeSpeed = 1.0f; // Velocidade do sumiço

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        _soliloquyText.text = "";
    }

    public void ShowText(string textContent)
    {
        StopAllCoroutines();
        StartCoroutine(Routine_TypewriterAndFade(textContent));
    }

    private IEnumerator Routine_TypewriterAndFade(string textContent)
    {
        // 1. Prepara o Canvas
        _canvasGroup.alpha = 1f;
        _soliloquyText.text = "";

        // 2. Efeito Typewriter (Letra por Letra)
        foreach (char c in textContent)
        {
            _soliloquyText.text += c;
            
            // Pausa um pouco mais se for pontuação (dá um ritmo humano à fala)
            if (c == '.' || c == ',' || c == '!' || c == '?')
                yield return new WaitForSeconds(_typingSpeed * 3f);
            else
                yield return new WaitForSeconds(_typingSpeed);
        }

        // 3. Aguarda o tempo de leitura do jogador
        yield return new WaitForSeconds(_displayDuration);

        // 4. Fade Out gótico e lento
        while (_canvasGroup.alpha > 0f)
        {
            _canvasGroup.alpha -= Time.deltaTime * _fadeSpeed;
            yield return null;
        }

        // 5. Limpa a tela e avisa o UIManager que terminou
        _soliloquyText.text = "";
        UIManager.Instance.OnSoliloquyFinished();
    }
}