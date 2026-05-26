using UnityEngine;

[DisallowMultipleComponent]
public class CameraFollow2D : MonoBehaviour
{
    [Header("Alvo")]
    [SerializeField] private Transform _target;

    [Header("Configurações Físicas")]
    [SerializeField] private float _smoothTime = 0.3f;
    [SerializeField] private Vector3 _offset = new Vector3(0f, 2f, -10f);

    private Vector3 _velocity = Vector3.zero;

    private void LateUpdate()
    {
        if (_target == null) return;

        Vector3 targetPosition = _target.position + _offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _velocity, _smoothTime);
    }
}