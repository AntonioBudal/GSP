using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewProvince", menuName = "Corvus/Province Config")]
public class ProvinceConfig : ScriptableObject
{
    public string id; // Ex: "LOTHIAN"
    public string provinceName;
    
    // O Grafo: quem faz fronteira com esta província
    public int requiredFocus = 1;
    public List<ProvinceConfig> neighbors = new List<ProvinceConfig>();
}