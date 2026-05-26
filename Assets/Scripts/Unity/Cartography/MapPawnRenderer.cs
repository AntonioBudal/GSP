using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapPawnRenderer : MonoBehaviour
{
    [Header("Dependências")]
    [SerializeField] private MapRenderer _mapRenderer;
    [SerializeField] private UI_MapPawn _pawnPrefab;
    [SerializeField] private RectTransform _pawnsContainer;

    private Dictionary<string, UI_MapPawn> _activePawns = new Dictionary<string, UI_MapPawn>();

    private void Start()
    {
        // Aguarda a inicialização do Bootstrap para assinar os eventos
        Invoke(nameof(BindEvents), 0.2f);
    }

    private void BindEvents()
    {
        if (GameBootstrap.Instance != null && GameBootstrap.Instance.StateController != null)
        {
            GameBootstrap.Instance.StateController.OnStateChanged += HandleStateChanged;
            GameBootstrap.Instance.Expeditions.OnExpeditionTick += HandleExpeditionTick;
            
            // Sincroniza caso o jogo tenha sido carregado de um Save
            SyncExistingExpeditions();
        }
    }

    private void SyncExistingExpeditions()
    {
        var activeExpeditions = GameBootstrap.Instance.Expeditions.GetAllActiveExpeditions();
        foreach (var runtime in activeExpeditions)
        {
            SpawnPawn(runtime.CrowId, runtime.TargetRegionId, runtime.Mission, runtime.DaysElapsed);
        }
    }

    private void HandleStateChanged(Crow crow)
    {
        bool hasPawn = _activePawns.ContainsKey(crow.ID);
        bool isExpeditionState = crow.CurrentState == CrowState.EmExpedicao;

        if (isExpeditionState && !hasPawn)
        {
            // O Estado mudou, mas precisamos consultar o Manager para saber ONDE ele está
            var runtime = GameBootstrap.Instance.Expeditions.GetAllActiveExpeditions()
                            .FirstOrDefault(e => e.CrowId == crow.ID);
            
            if (runtime != null)
            {
                SpawnPawn(crow.ID, runtime.TargetRegionId, runtime.Mission, runtime.DaysElapsed);
            }
        }
        else if (!isExpeditionState && hasPawn)
        {
            // O corvo morreu, voltou ou fadigou. Removemos o peão do mapa.
            RemovePawn(crow.ID);
        }
    }

    private void HandleExpeditionTick(ExpeditionProgressEvent progressEvent)
    {
        if (_activePawns.TryGetValue(progressEvent.CrowId, out var pawn))
        {
            pawn.UpdateProgress(progressEvent.DaysElapsed);
        }
    }

    private void SpawnPawn(string crowId, string targetRegionId, MissionType mission, int daysElapsed)
    {
        RectTransform targetNode = _mapRenderer.GetNodeRect(targetRegionId);
        if (targetNode == null) return;

        UI_MapPawn newPawn = Instantiate(_pawnPrefab, _pawnsContainer);
        RectTransform pawnRect = newPawn.GetComponent<RectTransform>();
        
        // Posiciona o peão exatamente em cima do nó, mas levemente deslocado para cima (Offset)
        // para não tampar o botão da região
        pawnRect.position = targetNode.position;
        pawnRect.anchoredPosition += new Vector2(0, 40f); 

        newPawn.Setup(crowId, mission);
        newPawn.UpdateProgress(daysElapsed);

        _activePawns.Add(crowId, newPawn);
    }

    private void RemovePawn(string crowId)
    {
        if (_activePawns.TryGetValue(crowId, out var pawn))
        {
            _activePawns.Remove(crowId);
            Destroy(pawn.gameObject);
        }
    }

    private void OnDestroy()
    {
        if (GameBootstrap.Instance != null)
        {
            if (GameBootstrap.Instance.StateController != null)
                GameBootstrap.Instance.StateController.OnStateChanged -= HandleStateChanged;
            
            if (GameBootstrap.Instance.Expeditions != null)
                GameBootstrap.Instance.Expeditions.OnExpeditionTick -= HandleExpeditionTick;
        }
    }
}