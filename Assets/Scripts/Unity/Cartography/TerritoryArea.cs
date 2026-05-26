using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class TerritoryArea : MonoBehaviour
{
    [Header("Metadados")]
    public string TerritoryID = "Irlanda";
    
    [Header("Geração")]
    [Tooltip("Distância (em pixels ou unidades) entre cada node fatiado.")]
    public float NodeSpacing = 50f;

    public PolygonCollider2D Polygon { get; private set; }

    private void Awake()
    {
        Polygon = GetComponent<PolygonCollider2D>();
    }
}