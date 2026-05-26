using System.Collections.Generic;
using UnityEngine;

// 1. A Estrutura Leve
public class LogicalNode
{
    public string ID { get; set; }
    public Vector2 LocalPosition { get; set; }
    public bool IsLand { get; set; }
    public string TerritoryID { get; set; }
    public List<LogicalNode> Neighbors { get; set; } = new List<LogicalNode>();
}