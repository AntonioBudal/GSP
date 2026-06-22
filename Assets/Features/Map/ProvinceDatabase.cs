using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ProvinceDatabase", menuName = "Corvus/Province Database")]
public class ProvinceDatabase : ScriptableObject
{
    public List<ProvinceConfig> provinces = new List<ProvinceConfig>();
}