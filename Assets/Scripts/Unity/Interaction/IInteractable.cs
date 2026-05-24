using UnityEngine;

public interface IInteractable
{
    // O texto ou comando contextual (ex: "Interagir", "Examinar", "Disciplinar")
    string InteractionPrompt { get; }

    // O que acontece quando o Padre puxa o gatilho
    void Interact();

    // Funções para ligar/desligar o indicador visual ("Pressione F")
    void ShowPrompt();
    void HidePrompt();
}