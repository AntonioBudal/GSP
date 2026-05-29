using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIWindow))]
public class UI_MapWindow : MonoBehaviour
{
    [Header("Controle de Janela")]
    [SerializeField] private Button _btnClose;
    private UIWindow _window;

    [Header("Geradores Opcionais")]
    [Tooltip("Arraste o gerador procedural se você quiser gerar o mapa apenas quando a janela abrir pela primeira vez.")]
    [SerializeField] private ProceduralMapGenerator _generator;
    private bool _hasGeneratedMap = false;

    private void Awake()
    {
        _window = GetComponent<UIWindow>();
        if (_btnClose != null)
        {
            _btnClose.onClick.RemoveAllListeners();
            _btnClose.onClick.AddListener(() => UIManager.Instance.CloseTopModal());
        }
    }

    public void SetupAndShow()
    {
        // Se o mapa procedural ainda não rodou, roda agora.
        if (!_hasGeneratedMap && _generator != null)
        {
            // Nota: Se você chamar GenerateLogicalMap() no Start do próprio gerador,
            // você não precisa chamar nada aqui, o mapa já nascerá desenhado.
            _hasGeneratedMap = true;
        }

        _window.Show();
    }
}