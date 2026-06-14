using System;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    // Acesso global direto e descomplicado
    public static TimeManager Instance { get; private set; }

    // Evento que os outros sistemas (Expedição, Treino, etc.) vão escutar
    public event Action OnDayAdvanced;

    // Mantemos o dia em memória durante a execução.
    // Na próxima fase (Save), sincronizaremos isso com o SaveData.
    public int CurrentDay { get; private set; } = 1;

    private void Awake()
    {
        // Garante que exista apenas uma instância deste Manager
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        // Gatilho de debug prático para testar o loop isoladamente
        // Pressione a barra de espaço para avançar 1 dia.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AdvanceDay();
        }
    }

    // Permite que o SaveManager injete o dia correto ao carregar o jogo
    public void SetCurrentDay(int loadedDay)
    {
        CurrentDay = loadedDay;
        Debug.Log($"[TimeManager] Dia sincronizado com o save: {CurrentDay}");
    }

    public void AdvanceDay()
    {
        CurrentDay++;
        Debug.Log($"[TimeManager] Novo dia iniciado: Dia {CurrentDay}");
        
        // Dispara o evento para notificar quem estiver escutando (?. previne erro se ninguém estiver)
        OnDayAdvanced?.Invoke();
    }
}