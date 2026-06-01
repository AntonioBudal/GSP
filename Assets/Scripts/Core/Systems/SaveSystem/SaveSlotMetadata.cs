// Assets/Scripts/Core/SaveSystem/SaveSlotMetadata.cs
using System;

namespace Corvus.Core.SaveSystem
{
    /// <summary>
    /// Contrato de dados ultra-leve. Lido pela UI do Menu Principal para popular os slots de 0 a 9.
    /// </summary>
    public class SaveSlotMetadata
    {
        public int SlotID { get; set; }
        public int CurrentDay { get; set; }
        public DateTime LastPlayedDate { get; set; }
        public string NarrativeStatus { get; set; } // Ex: "A linhagem sobrevive"
    }
}