// Assets/Scripts/Unity/TimeTicker.cs
using UnityEngine;
using System.Collections;

public class TimeTicker : MonoBehaviour
{
    [Header("Configurações de Tempo")]
    [Tooltip("Se ativado, o tempo avança sozinho.")]
    public bool autoAdvance = false;
    
    [Tooltip("Quantos segundos reais equivalem a 1 dia no jogo.")]
    public float secondsPerDay = 2.0f;

    private Coroutine _tickerCoroutine;

    private void Start()
    {
        if (autoAdvance)
        {
            StartAutoTicker();
        }
    }

    private void Update()
    {
        // Debug Manual: Avança exatamente 1 dia ao apertar Espaço
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AdvanceOneDay();
        }

        // Debug Toggle: Liga/desliga a passagem de tempo automática com a tecla T
        if (Input.GetKeyDown(KeyCode.T))
        {
            autoAdvance = !autoAdvance;
            
            if (autoAdvance) StartAutoTicker();
            else StopAutoTicker();
            
            Debug.Log($"[TimeTicker] Avanço Automático: {(autoAdvance ? "LIGADO" : "DESLIGADO")}");
        }
    }

    private void StartAutoTicker()
    {
        if (_tickerCoroutine == null)
        {
            _tickerCoroutine = StartCoroutine(TickerLoop());
        }
    }

    private void StopAutoTicker()
    {
        if (_tickerCoroutine != null)
        {
            StopCoroutine(_tickerCoroutine);
            _tickerCoroutine = null;
        }
    }

    private IEnumerator TickerLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(secondsPerDay);
            AdvanceOneDay();
        }
    }

    private void AdvanceOneDay()
    {
        // Busca a instância do Bootstrap e avança o relógio purista
        if (GameBootstrap.Instance != null && GameBootstrap.Instance.Clock != null)
        {
            GameBootstrap.Instance.Clock.AdvanceTime(1);
        }
        else
        {
            Debug.LogWarning("[TimeTicker] O Relógio não foi encontrado. O Bootstrap falhou?");
        }
    }
}