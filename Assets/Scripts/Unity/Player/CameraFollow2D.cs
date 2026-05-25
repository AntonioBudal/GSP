using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Alvo")]
    [SerializeField] private Transform _target; // Arraste o Priest_Player aqui

    [Header("Configurações Físicas")]
    [SerializeField] private float _smoothTime = 0.3f; // Quão "pesada" é a câmera
    [SerializeField] private Vector3 _offset = new Vector3(0f, 2f, -10f); // Altura e distância

    private Vector3 _velocity = Vector3.zero;

    private void LateUpdate()
    {
        if (_target == null) return;

        // Posição desejada
        Vector3 targetPosition = _target.position + _offset;

        // Interpolação suave nativa da Unity
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _velocity, _smoothTime);
    }
}