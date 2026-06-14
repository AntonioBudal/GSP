using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewProvince", menuName = "Corvus/Province Config")]
public class ProvinceConfig : ScriptableObject
{
    public string id; // Ex: "PROV_01"
    public string provinceName;
    
    // Lista de conexões. Você vai arrastar outras ProvinceConfigs aqui pelo Unity!
    public List<ProvinceConfig> neighbors = new List<ProvinceConfig>();
}