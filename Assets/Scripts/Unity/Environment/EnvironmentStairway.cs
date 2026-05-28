using System.Collections;
using UnityEngine;

public class EnvironmentStairway : BaseInteractable
{
    [Header("Navegação")]
    [Tooltip("O ponto vazio (Waypoint) onde o Padre vai nascer após usar a escada.")]
    [SerializeField] private Transform _destinationWaypoint;

    [Header("Suco Visual (Juice)")]
    [Tooltip("Tempo que leva para o Padre subir as escadas (simula a caminhada no escuro).")]
    [SerializeField] private float _transitionDuration = 0.2f;

    public override void Interact()
    {
        if (_destinationWaypoint == null)
        {
            Debug.LogWarning("[Escadaria] O destino não foi configurado no Inspector!");
            return;
        }

        StartCoroutine(Routine_Transition());
    }

    private IEnumerator Routine_Transition()
    {
        // 1. Opcional: Se você tiver uma tela preta de Fade no UIManager, chame o FadeOut aqui.
        // UIManager.Instance.FadeOut();

        // 2. Opcional: Bloqueie o movimento do Padre para ele não andar durante o teleporte.
        // Se o seu Padre tiver um script como PlayerController, você pode desativá-lo.
        
        yield return new WaitForSeconds(_transitionDuration);

        // 3. Encontra o Padre e o move para o andar de cima/baixo
        GameObject padre = GameObject.FindGameObjectWithTag("Player");
        if (padre != null)
        {
            padre.transform.position = _destinationWaypoint.position;
        }

        // 4. Opcional: Chame o FadeIn da tela preta para revelar o novo andar.
        // UIManager.Instance.FadeIn();
    }
}