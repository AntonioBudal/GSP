using System;
using UnityEngine;

public class RavenManager : MonoBehaviour
{
    public static RavenManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        // Atalho de debug: Pressione 'C' para criar um corvo com atributos aleatórios
        if (Input.GetKeyDown(KeyCode.C))
        {
            // Gerando atributos base simples para o teste
            int randomSpeed = UnityEngine.Random.Range(1, 4);
            int randomLifespan = UnityEngine.Random.Range(10, 20);
            int randomFocus = UnityEngine.Random.Range(1, 4);
            
            CreateRaven(randomSpeed, randomLifespan, randomFocus);
        }

        // Atalho de debug: Pressione 'R' para listar os corvos no console
        if (Input.GetKeyDown(KeyCode.R))
        {
            PrintAllRavens();
        }
    }

    // Método central para injetar um novo corvo no sistema
    public void CreateRaven(int speed, int lifespan, int focus)
    {
        // Gera um ID único curto e legível para facilitar auditoria nos logs/JSON
        string newId = "RVN_" + Guid.NewGuid().ToString().Substring(0, 5).ToUpper();
        
        RavenData newRaven = new RavenData(newId, speed, lifespan, focus);
        
        // Insere o corvo diretamente na lista do SaveData
        SaveManager.Instance.CurrentSave.ravens.Add(newRaven);
        
        Debug.Log($"[RavenManager] Corvo Criado: {newId} | Vel: {speed}, Vida: {lifespan}, Foco: {focus}. Total agora: {SaveManager.Instance.CurrentSave.ravens.Count}");
    }

    // Utilitário para verificar o estado da colônia
    private void PrintAllRavens()
    {
        var ravenList = SaveManager.Instance.CurrentSave.ravens;
        Debug.Log($"--- Relatório do Berçário: {ravenList.Count} Corvos ---");
        
        foreach (var raven in ravenList)
        {
            Debug.Log($"- ID: {raven.id} | Estado: {raven.state} | Vel: {raven.speed}, Vida: {raven.lifespan}, Foco: {raven.focus}");
        }
    }
}